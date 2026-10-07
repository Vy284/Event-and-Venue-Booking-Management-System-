using EventVenueBooking.Database;
using EventVenueBooking.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace EventVenueBooking.Services
{
    // ===== Hằng số (khớp comment trong các Entity) =====
    public static class BookingStatuses
    {
        public const byte Pending = 0, Confirmed = 1, InProgress = 2, Completed = 3, Cancelled = 4;
    }

    public static class RentalUnits
    {
        public const byte Hour = 0, Session = 1, Day = 2;

        // Session = 4 giờ, Day = 8 giờ
        public static int HoursPerUnit(byte unit)
        {
            switch (unit)
            {
                case Hour: return 1;
                case Session: return 4;
                case Day: return 8;
                default: throw new ArgumentOutOfRangeException("unit");
            }
        }
    }

    public static class PaymentTypes
    {
        public const byte Deposit = 0, Final = 1;
    }

    public static class PaymentStatuses
    {
        public const byte Pending = 0, Completed = 1, Refunded = 2;
    }

    public static class VenueStatuses
    {
        public const byte Inactive = 0, Active = 1, UnderMaintenance = 2;
    }

    // ===== DTO =====
    public class BookingException : Exception
    {
        public BookingException(string message) : base(message) { }
    }

    public class AddOnRequest
    {
        public int AddOnId { get; set; }
        public int Quantity { get; set; }
    }

    public class AddOnLine
    {
        public int AddOnId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class PriceQuote
    {
        public decimal VenueRate { get; set; }
        public byte RentalUnit { get; set; }
        public int RentalQuantity { get; set; }
        public decimal VenueCost { get; set; }
        public List<AddOnLine> AddOnLines { get; set; } = new List<AddOnLine>();
        public decimal AddOnCost { get; set; }
        public decimal TotalCost { get { return VenueCost + AddOnCost; } }
    }

    // ===== Service =====
    public class BookingService
    {
        public const decimal DepositRate = 0.30m;   // tỉ lệ cọc - chốt lại với nhóm

        private readonly ApplicationDbContext _db;

        public BookingService(ApplicationDbContext db)
        {
            _db = db;
        }

        // 1. Số đơn vị thuê (làm tròn lên)
        public static int CalculateRentalQuantity(byte unit, DateTime start, DateTime end)
        {
            double hours = (end - start).TotalHours;
            return (int)Math.Ceiling(hours / RentalUnits.HoursPerUnit(unit));
        }

        // 2. Kiểm tra trùng lịch cùng venue (booking Cancelled không tính)
        public bool HasOverlap(int venueId, DateTime start, DateTime end, int? excludeBookingId = null)
        {
            int exclude = excludeBookingId ?? 0;
            return _db.Bookings.Any(b =>
                b.VenueId == venueId &&
                b.Status != BookingStatuses.Cancelled &&
                b.EventStartDateTime < end &&
                b.EventEndDateTime > start &&
                b.BookingId != exclude);
        }

        // 3. Tính tiền (không lưu DB, dùng được cho trang xem trước giá)
        public PriceQuote CalculatePrice(Venue venue, DateTime start, DateTime end, IEnumerable<AddOnRequest> addOns)
        {
            if (end <= start)
                throw new BookingException("Thời gian kết thúc phải sau thời gian bắt đầu.");

            var quote = new PriceQuote
            {
                VenueRate = venue.RentalRate,
                RentalUnit = venue.RentalUnit,
                RentalQuantity = CalculateRentalQuantity(venue.RentalUnit, start, end)
            };
            quote.VenueCost = quote.VenueRate * quote.RentalQuantity;

            var requested = (addOns ?? new List<AddOnRequest>())
                .Where(a => a.Quantity > 0)
                .GroupBy(a => a.AddOnId)
                .Select(g => new { AddOnId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToList();

            var ids = requested.Select(r => r.AddOnId).ToList();
            var services = _db.AddOnServices
                .Where(s => ids.Contains(s.AddOnId) && s.IsActive)
                .ToDictionary(s => s.AddOnId);

            foreach (var r in requested)
            {
                AddOnService svc;
                if (!services.TryGetValue(r.AddOnId, out svc))
                    throw new BookingException("Dịch vụ đi kèm không tồn tại hoặc đã ngừng cung cấp.");

                quote.AddOnLines.Add(new AddOnLine
                {
                    AddOnId = svc.AddOnId,
                    Name = svc.Name,
                    Quantity = r.Quantity,
                    UnitPrice = svc.Price,          // giá hiện tại lấy từ DB, không tin giá từ form
                    Subtotal = svc.Price * r.Quantity
                });
            }
            quote.AddOnCost = quote.AddOnLines.Sum(l => l.Subtotal);
            return quote;
        }

        // 4. Tạo booking (trạng thái Pending)
        public Booking CreateBooking(int clientUserId, int venueId, int eventTypeId, int guestCount,
                                     DateTime start, DateTime end, IEnumerable<AddOnRequest> addOns)
        {
            if (end <= start)
                throw new BookingException("Thời gian kết thúc phải sau thời gian bắt đầu.");
            if (start < DateTime.Now)
                throw new BookingException("Thời gian bắt đầu phải ở tương lai.");
            if (guestCount < 1)
                throw new BookingException("Số lượng khách phải lớn hơn 0.");

            var client = _db.Users.Find(clientUserId);
            if (client == null || !client.IsActive)
                throw new BookingException("Tài khoản khách hàng không hợp lệ.");
            if (_db.EventTypes.Find(eventTypeId) == null)
                throw new BookingException("Loại sự kiện không hợp lệ.");

            using (var tx = _db.Database.BeginTransaction())
            {
                // Khóa dòng Venue để 2 người đặt cùng lúc phải xếp hàng.
                // "Venues" là tên bảng EF tạo mặc định; sai thì sửa theo DB.
                _db.Database.ExecuteSqlCommand(
                    "SELECT VenueId FROM Venues WITH (UPDLOCK, ROWLOCK) WHERE VenueId = @p0", venueId);

                var venue = _db.Venues.Find(venueId);
                if (venue == null)
                    throw new BookingException("Địa điểm không tồn tại.");
                if (venue.Status != VenueStatuses.Active)
                    throw new BookingException("Địa điểm hiện không nhận đặt.");
                if (guestCount > venue.Capacity)
                    throw new BookingException("Số khách vượt quá sức chứa (" + venue.Capacity + ").");
                if (HasOverlap(venueId, start, end))
                    throw new BookingException("Địa điểm đã có người đặt trong khoảng thời gian này.");

                var quote = CalculatePrice(venue, start, end, addOns);

                var booking = new Booking
                {
                    ClientUserId = clientUserId,
                    VenueId = venueId,
                    EventTypeId = eventTypeId,
                    GuestCount = guestCount,
                    EventStartDateTime = start,
                    EventEndDateTime = end,
                    Status = BookingStatuses.Pending,

                    // Snapshot giá tại thời điểm đặt
                    VenueRateAtBooking = quote.VenueRate,
                    RentalUnitAtBooking = quote.RentalUnit,
                    RentalQuantity = quote.RentalQuantity,
                    VenueCost = quote.VenueCost,
                    AddOnCost = quote.AddOnCost,
                    TotalCost = quote.TotalCost,
                    CreatedAt = DateTime.Now
                };

                foreach (var line in quote.AddOnLines)
                {
                    booking.BookingAddOns.Add(new BookingAddOn
                    {
                        AddOnId = line.AddOnId,
                        Quantity = line.Quantity,
                        PriceAtBooking = line.UnitPrice,
                        Subtotal = line.Subtotal
                    });
                }

                _db.Bookings.Add(booking);
                _db.SaveChanges();
                tx.Commit();
                return booking;
            }
        }

        // 5. Ghi nhận thanh toán (cọc = DepositRate * TotalCost; Final = phần còn lại)
        //    SỬA: có transaction + khóa dòng booking (chống ghi trùng khi bấm 2 lần),
        //    chặn booking Completed, làm tròn tiền cọc chuẩn, kiểm tra phương thức thanh toán.
        public Payment RecordPayment(int bookingId, byte paymentType, string method, int recordedByUserId)
        {
            method = (method ?? "").Trim();
            if (method.Length == 0)
                throw new BookingException("Vui lòng chọn phương thức thanh toán.");
            if (method.Length > 50)
                throw new BookingException("Phương thức thanh toán quá dài (tối đa 50 ký tự).");

            using (var tx = _db.Database.BeginTransaction())
            {
                // Khóa dòng booking để bấm 2 lần / 2 request song song không ghi trùng.
                // "Bookings" là tên bảng EF tạo mặc định; sai thì sửa theo DB.
                _db.Database.ExecuteSqlCommand(
                    "SELECT BookingId FROM Bookings WITH (UPDLOCK, ROWLOCK) WHERE BookingId = @p0", bookingId);

                var booking = _db.Bookings.Include(b => b.Payments).FirstOrDefault(b => b.BookingId == bookingId);
                if (booking == null)
                    throw new BookingException("Booking không tồn tại.");
                if (booking.Status == BookingStatuses.Cancelled)
                    throw new BookingException("Booking đã bị hủy.");
                if (booking.Status == BookingStatuses.Completed)
                    throw new BookingException("Booking đã hoàn tất.");

                decimal paid = booking.Payments
                    .Where(p => p.PaymentStatus == PaymentStatuses.Completed)
                    .Sum(p => p.Amount);

                decimal amount;
                if (paymentType == PaymentTypes.Deposit)
                {
                    if (booking.Payments.Any(p => p.PaymentType == PaymentTypes.Deposit && p.PaymentStatus == PaymentStatuses.Completed))
                        throw new BookingException("Booking này đã đặt cọc.");
                    amount = Math.Round(booking.TotalCost * DepositRate, 0, MidpointRounding.AwayFromZero);
                }
                else if (paymentType == PaymentTypes.Final)
                {
                    if (booking.Status == BookingStatuses.Pending)
                        throw new BookingException("Cần đặt cọc trước khi thanh toán phần còn lại.");
                    amount = booking.TotalCost - paid;
                    if (amount <= 0)
                        throw new BookingException("Booking đã thanh toán đủ.");
                }
                else
                {
                    throw new BookingException("Loại thanh toán không hợp lệ.");
                }

                var payment = new Payment
                {
                    BookingId = bookingId,
                    Amount = amount,
                    PaymentType = paymentType,
                    PaymentMethod = method,
                    PaymentStatus = PaymentStatuses.Completed,   // thanh toán giả lập: coi như thành công ngay
                    PaymentDate = DateTime.Now,
                    RecordedByUserId = recordedByUserId
                };
                _db.Payments.Add(payment);

                if (paymentType == PaymentTypes.Deposit && booking.Status == BookingStatuses.Pending)
                {
                    booking.Status = BookingStatuses.Confirmed;
                    booking.LastModifiedByUserId = recordedByUserId;
                    booking.LastModifiedAt = DateTime.Now;
                }

                _db.SaveChanges();
                tx.Commit();
                return payment;
            }
        }

        // 6. Hủy booking (hủy thì các khoản đã thu chuyển Refunded - chốt chính sách với nhóm)
        public void CancelBooking(int bookingId, int cancelledByUserId, string reason)
        {
            var booking = _db.Bookings.Include(b => b.Payments).FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null)
                throw new BookingException("Booking không tồn tại.");
            if (booking.Status != BookingStatuses.Pending && booking.Status != BookingStatuses.Confirmed)
                throw new BookingException("Chỉ có thể hủy booking đang chờ hoặc đã xác nhận.");

            booking.Status = BookingStatuses.Cancelled;
            booking.CancelledByUserId = cancelledByUserId;
            booking.CancelledAt = DateTime.Now;
            booking.CancelReason = reason;

            foreach (var p in booking.Payments.Where(p => p.PaymentStatus == PaymentStatuses.Completed))
                p.PaymentStatus = PaymentStatuses.Refunded;

            _db.SaveChanges();
        }

        // 7. Đổi trạng thái (Coordinator/Admin): Pending->Confirmed->InProgress->Completed
        public void ChangeStatus(int bookingId, byte newStatus, int modifiedByUserId)
        {
            var booking = _db.Bookings.Include(b => b.Payments).FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null)
                throw new BookingException("Booking không tồn tại.");

            bool allowed =
                (booking.Status == BookingStatuses.Pending && newStatus == BookingStatuses.Confirmed) ||
                (booking.Status == BookingStatuses.Confirmed && newStatus == BookingStatuses.InProgress) ||
                (booking.Status == BookingStatuses.InProgress && newStatus == BookingStatuses.Completed);
            if (!allowed)
                throw new BookingException("Không thể chuyển trạng thái booking theo cách này.");

            if (newStatus == BookingStatuses.Completed)
            {
                decimal paid = booking.Payments
                    .Where(p => p.PaymentStatus == PaymentStatuses.Completed)
                    .Sum(p => p.Amount);
                if (paid < booking.TotalCost)
                    throw new BookingException("Chưa thanh toán đủ, không thể hoàn tất booking.");
            }

            booking.Status = newStatus;
            booking.LastModifiedByUserId = modifiedByUserId;
            booking.LastModifiedAt = DateTime.Now;
            _db.SaveChanges();
        }
    }
}
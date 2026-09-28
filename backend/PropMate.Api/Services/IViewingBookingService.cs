using System.Collections.Generic;
using System.Threading.Tasks;
using PropMate.Api.DTOs.Viewing;

namespace PropMate.Api.Services
{
    public interface IViewingBookingService
    {
        Task<ViewingBookingDto> BookSlotAsync(int buyerId, int viewingSlotId);
        Task<IEnumerable<ViewingBookingDto>> GetMyBookingsAsync(int buyerId);
        Task<bool> CancelBookingAsync(int buyerId, int bookingId);
        Task<IEnumerable<ViewingBookingDto>> GetOwnerBookingsAsync(int ownerId);
        Task<bool> CompleteBookingAsync(int ownerId, int bookingId);
    }
}

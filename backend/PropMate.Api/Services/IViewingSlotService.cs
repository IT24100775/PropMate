using System.Collections.Generic;
using System.Threading.Tasks;
using PropMate.Api.DTOs.Viewing;

namespace PropMate.Api.Services
{
    public interface IViewingSlotService
    {
        Task<IEnumerable<ViewingSlotDto>> GetOwnerSlotsAsync(int ownerId, int propertyListingId);
        Task<ViewingSlotDto> CreateSlotAsync(int ownerId, int propertyListingId, CreateViewingSlotDto dto);
        Task<bool> DeleteSlotAsync(int ownerId, int slotId);
        Task<IEnumerable<ViewingSlotDto>> GetAvailableSlotsAsync(int propertyListingId);
    }
}

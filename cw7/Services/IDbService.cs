using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using cw7.Models.DTO;

namespace cw7.Services
{
    public interface IDbService
    {
        Task<IEnumerable<SomeSortOfTrip>> GetTrips();
        Task RemoveClient(int id);

        Task<bool> CheckIfClientExists(int id);

        Task <int>InsertClient(InsertClientRequest request, int idTrip);

        Task<bool> CheckIfPeselExists(string pesel);

        Task<bool> CheckIfClientIsAssigned(int idClient, int idTrip);
        
        Task<bool> CheckIfTripExists (int idTrip);
    }
}

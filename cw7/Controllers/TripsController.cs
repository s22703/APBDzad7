using System;
using System.Threading.Tasks;
using cw7.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using cw7.Models;
using cw7.Models.DTO;

namespace cw7.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly IDbService _dbService;

        public TripsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrips()
        {
            var trips = await _dbService.GetTrips();
            return Ok(trips);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteTrips(int id)
        {
            if (await _dbService.CheckIfClientExists(id))
                return BadRequest("Given client cannot be removed");

            await _dbService.RemoveClient(id);
            return Ok($"Client with id: {id} was removed");
        }

        [HttpPost("{idTrip}/clients")]
       
        public async Task<IActionResult> AssignClientToTrip(int idTrip,InsertClientRequest request)
        {
            if (await _dbService.CheckIfTripExists(idTrip))
                return BadRequest("Given trip already exists in DB");
            var id = 0;
            try
            {
                id = await _dbService.InsertClient(request, idTrip);
            }
            catch (Exception e)
            {
                if (e.Message.Equals("Client is already assigned to this trip"))
                    return BadRequest(e.Message);
                else
                    return NotFound(e.Message);
                
            }

            return Ok($"Client with id: {id} was added to trip with id {idTrip}");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using cw7.Models;
using cw7.Models.DTO;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;

namespace cw7.Services
{
    public class DbService : IDbService
    {
        private readonly _2019SBDContext _dbContext;

        public DbService(_2019SBDContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<SomeSortOfTrip>> GetTrips()
        {
            return await _dbContext.Trips
                .Include(e => e.CountryTrips)
                .Include(e => e.ClientTrips)
                .Select(e => new SomeSortOfTrip()
                {
                    Name = e.Name,
                    Description = e.Description,
                    MaxPeople = e.MaxPeople,
                    DateFrom = e.DateFrom,
                    DateTo = e.DateTo,
                    Countries = e.CountryTrips.Select(e => new SomeSortOfCountry()
                    {
                        Name = e.IdCountryNavigation.Name
                    }).ToList(),
                    Clients = e.ClientTrips.Select(e => new SomeSortOfClient()
                    {
                        FirstName = e.IdClientNavigation.FirstName,
                        LastName = e.IdClientNavigation.LastName 
                    }).ToList()    
                }).OrderByDescending(x => x.DateFrom).ToListAsync();
        }

        public async Task RemoveClient(int id)
        {
            var context = _dbContext;
            

            var clientToRemove = await context.Clients.FirstAsync(e => e.IdClient == id);

            context.Clients.Remove(clientToRemove);
            await _dbContext.SaveChangesAsync();

        }

        public async Task<bool> CheckIfClientExists(int id)
        {
            var context = _dbContext;

            List <ClientTrip> clientTripList = await context.ClientTrips.Where(e => e.IdClient == id).ToListAsync();

            if (clientTripList.Count > 0)
                return true;


            return false;
        }

        public async Task<int> InsertClient(InsertClientRequest request, int idTrip)
        {
            var context = _dbContext;

            var newClient = await _dbContext.Clients.Where(e => e.Pesel == request.Pesel).FirstOrDefaultAsync();

            if (!await CheckIfPeselExists(request.Pesel))
            {
                 newClient = new Client()
                {
                    IdClient = context.Clients.Max(c => c.IdClient) + 1,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Telephone = request.Telephone,
                    Pesel = request.Pesel,
                };
                await context.Clients.AddAsync(newClient);
            }

            if (await CheckIfClientIsAssigned(newClient.IdClient, idTrip))
                throw new Exception("Client is already assigned to this trip");

            var trip = await _dbContext.Trips.Where(e => e.IdTrip == idTrip).FirstOrDefaultAsync();
            var newClientTrip = (new ClientTrip()
            {
                IdClientNavigation = newClient,
                IdTrip = request.IdTrip,
                IdClient = newClient.IdClient,
                IdTripNavigation = trip,
                PaymentDate = DateTime.Now,
                RegisteredAt = DateTime.Now
                
            });
            await _dbContext.ClientTrips.AddAsync(newClientTrip);
            newClient.ClientTrips.Add(newClientTrip);
            trip.ClientTrips.Add(newClientTrip);
            await _dbContext.SaveChangesAsync();
            return newClient.IdClient;


        }

        public async Task<bool> CheckIfPeselExists(string pesel)
        {
            var context = _dbContext;

            var peselQuery = await context.Clients.AnyAsync(e => e.Pesel == pesel);

            return peselQuery;
        }

        public async Task<bool> CheckIfClientIsAssigned(int idClient, int idTrip)
        {
            var context = _dbContext;

            var tripQuery = await _dbContext.ClientTrips.AnyAsync(e => e.IdClient == idClient && e.IdTrip == idTrip);

            return tripQuery;
        }

        public async Task<bool> CheckIfTripExists(int idTrip)
        {
            var context = _dbContext;

            var tripQuery = await _dbContext.Trips.AnyAsync(e => e.IdTrip == idTrip);

            return tripQuery;
        }
    }
}

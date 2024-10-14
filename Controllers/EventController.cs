﻿using EventVault.Models;
using EventVault.Models.DTOs;
using EventVault.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventVault.Controllers
{
    [Route("[Controller]")]
    public class EventController : Controller
    {
        private readonly IEventServices _eventServices;

        public EventController(IEventServices eventServices)
        {
            _eventServices = eventServices;
        }

        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _eventServices.GetAllEventsAsync();

            if (events != null) {

                return Ok(events);
            }

            else
            {
                return NotFound("No events in database.");
            }

        }

        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddEventToDb(EventCreateDTO eventCreateDTO)
        {
            var eventToAdd = new Event {

                //add whatever is requred in eventobject contains.
            
            };

            var isSuccessfull = await _eventServices.AddEventToDbAsync(eventCreateDTO);

            if (isSuccessfull)
            {
                return Ok("Event Added");
            }

            else
            {
                return BadRequest();
            }
        }
    }
}
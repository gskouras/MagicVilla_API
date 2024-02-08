using MagicVilla_VillaAPI.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MagicVilla_VillaAPI.Models.Dto;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/villa")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    { 
        [HttpGet]
        public ActionResult<IEnumerable<VillaDTO>> GeVillas()
        {
            return Ok(VillaStore.villaList);
        }

        [HttpGet("{id:int}", Name="GetVilla")]
        public ActionResult<VillaDTO> GetVilla(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            if (villa == null)
            {
                return NotFound();
            }

            return Ok(villa);
        }

        [HttpPost]
        public ActionResult<VillaDTO> CreateVilla([FromBody] VillaDTO villa)
        {
            if (villa == null)
            {
                return BadRequest(villa);
            }

            villa.Id = VillaStore.villaList.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
            VillaStore.villaList.Add(villa);

            return CreatedAtRoute("GetVilla", new { id = villa.Id},villa);
        }
    }
}

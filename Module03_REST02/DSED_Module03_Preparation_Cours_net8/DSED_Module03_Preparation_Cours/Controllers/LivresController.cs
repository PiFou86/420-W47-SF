using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSED_Module03_Preparation_Cours.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DSED_Module03_Preparation_Cours.Controllers
{
    [Route("api/livres")]
    [ApiController]
    public class LivresController : ControllerBase
    {
        public static List<Livre> Donnees { set; get; } = new List<Livre>()
        {
            new Livre() { LivreId = 1, Titre = "Homo deus : une brève histoire de l'avenir"},
            new Livre() { LivreId = 2, Titre = "Les fourmis"},
            new Livre() { LivreId = 3, Titre = "Clean Code"},
        };

        // GET: api/livres
        [HttpGet]
        [ProducesResponseType(200)]
        public ActionResult<IEnumerable<Livre>> Get()
        {
            return Ok(Donnees);
        }

        // GET: api/livres/5
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ActionResult<Livre> Get(int id)
        {
            var livre = Donnees.Where(l => l.LivreId == id).SingleOrDefault();

            if (livre != null)
            {
                return Ok(livre);
            }

            return NotFound();
        }

        // POST: api/livres
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public ActionResult Post([FromBody] Livre livre)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var idMax = Donnees.OrderByDescending(l => l.LivreId).FirstOrDefault()?.LivreId ?? 0;
            livre.LivreId = idMax + 1;
            Donnees.Add(livre);

            return CreatedAtAction(nameof(Get), new { id = livre.LivreId }, livre);
        }

        // PUT: api/livres/5
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public ActionResult Put(int id, [FromBody] Livre livre)
        {
            if (!ModelState.IsValid || livre.LivreId != id)
            {
                return BadRequest();
            }

            int index = Donnees.FindIndex(l => l.LivreId == id);

            if (index < 0)
            {
                return NotFound();
            }

            Donnees[index] = livre;

            return NoContent();
        }

        // DELETE: api/livres/5
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public ActionResult Delete(int id)
        {
            var livre = Donnees.Where(l => l.LivreId == id).SingleOrDefault();

            if (livre == null)
            {
                return NotFound();
            }

            Donnees.Remove(livre);

            return NoContent();
        }
    }
}

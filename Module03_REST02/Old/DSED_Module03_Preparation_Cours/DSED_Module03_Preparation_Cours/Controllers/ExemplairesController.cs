using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSED_Module03_Preparation_Cours.Data;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DSED_Module03_Preparation_Cours.Controllers
{
    [Route("api/livres/{livreId}/exemplaires")]
    [ApiController]
    public class ExemplairesController : ControllerBase
    {
        public static Dictionary<int, List<Exemplaire>> Donnees { set; get; } = new Dictionary<int, List<Exemplaire>>()
        {
            {1, new List<Exemplaire>() {
                    new Exemplaire() { ExemplaireId = 1, LivreId = 1, EstEmprunte = false},
                    new Exemplaire() { ExemplaireId = 2, LivreId = 1, EstEmprunte = false},
                    new Exemplaire() { ExemplaireId = 3, LivreId = 1, EstEmprunte = true},
                    new Exemplaire() { ExemplaireId = 4, LivreId = 1, EstEmprunte = false}
               }
            },
            {2, new List<Exemplaire>() {
                    new Exemplaire() { ExemplaireId = 11, LivreId = 2, EstEmprunte = false},
                    new Exemplaire() { ExemplaireId = 12, LivreId = 2, EstEmprunte = false},
               }
            },
            {3, new List<Exemplaire>() {
                    new Exemplaire() { ExemplaireId = 22, LivreId = 3, EstEmprunte = false},
                    new Exemplaire() { ExemplaireId = 23, LivreId = 3, EstEmprunte = true},
                    new Exemplaire() { ExemplaireId = 24, LivreId = 3, EstEmprunte = false}
               }
            }
        };
        // GET: api/livres/1/exemplaires
        [HttpGet]
        [ProducesResponseType(200)]
        public ActionResult<IEnumerable<Exemplaire>> Get(int livreId)
        {
            if (!Donnees.ContainsKey(livreId))
            {
                return NotFound();
            }

            return Ok(Donnees[livreId]);
        }

        // GET api/livres/1/exemplaires/5
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ActionResult<Exemplaire> Get(int livreId, int id)
        {
            if (!Donnees.ContainsKey(livreId))
            {
                return NotFound();
            }

            Exemplaire exemplaire = Donnees[livreId].Where(e => e.ExemplaireId == id).SingleOrDefault();

            if (exemplaire is null)
            {
                return NotFound();
            }

            return Ok(exemplaire);
        }

        // POST api/livres/1/exemplaires
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public ActionResult Post(int livreId, [FromBody] Exemplaire exemplaire)
        {
            if (!ModelState.IsValid || exemplaire.LivreId != livreId)
            {
                return BadRequest();
            }

            if (!LivresController.Donnees.Any(l => l.LivreId == livreId))
            {
                return BadRequest();
            }

            var idMax = Donnees.Values.SelectMany(ce => ce.Select(e => e.ExemplaireId)).OrderByDescending(id => id).FirstOrDefault();

            exemplaire.ExemplaireId = idMax + 1;

            if (!Donnees.ContainsKey(livreId ))
            {
                Donnees.Add(livreId, new List<Exemplaire>());
            }
            Donnees[livreId].Add(exemplaire);

            return CreatedAtAction(nameof(Get), new { id = exemplaire.ExemplaireId }, exemplaire);
        }

        // PUT api/livres/1/exemplaires/5
        [HttpPut("{id}")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public ActionResult Put(int livreId, int id, [FromBody] Exemplaire exemplaire)
        {
            if (!ModelState.IsValid || exemplaire.LivreId != livreId)
            {
                return BadRequest();
            }

            if (!Donnees.ContainsKey(livreId))
            {
                return NotFound();
            }

            int index = Donnees[livreId].FindIndex(e => e.ExemplaireId == id);
            if (index < 0)
            {
                return NotFound();
            }

            Donnees[livreId][index] = exemplaire;

            return NoContent();
        }

        // DELETE api/livres/1/exemplaires/5
        [HttpDelete("{id}")]
        [ProducesResponseType(201)]
        [ProducesResponseType(404)]
        public ActionResult Delete(int livreId, int id)
        {
            if (!Donnees.ContainsKey(livreId))
            {
                return NotFound();
            }

            int index = Donnees[livreId].FindIndex(e => e.ExemplaireId == id);
            if (index < 0)
            {
                return NotFound();
            }

            Donnees[livreId].RemoveAt(index);

            return NoContent();
        }
    }
}

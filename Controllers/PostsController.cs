using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using monster_assignment.Models;
using Newtonsoft.Json;

namespace monster_assignment.Controllers
{
    [Route("api/[controller]")]
    public class PostsController : Controller
    {
        private readonly string POST_DB_PATH = "data/posts.json";
        private List<Post> _posts;

        public PostsController()
        {
            using (StreamReader r = new StreamReader(POST_DB_PATH))
            {
                // load json data from file
                string json = r.ReadToEnd();
                _posts = JsonConvert.DeserializeObject<List<Post>>(json);
            }
        }

        private void updateDB()
        {
            // update data on json file
            using (StreamWriter sw = new StreamWriter(@POST_DB_PATH))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                new JsonSerializer().Serialize(writer, _posts);
            }
        }

        // GET api/posts
        [HttpGet]
        public IEnumerable<Post> GetAll()
        {
            return _posts;
        }

        // GET api/posts/5
        [HttpGet("{id}", Name = "GetPost")]
        public IActionResult GetById(int id)
        {
            var post = _posts.FirstOrDefault(p => p.id == id);
            if (post == null)
            {
                return NotFound();
            }
            return new ObjectResult(post);
        }

        // POST api/posts
        [HttpPost]
        public IActionResult Create([FromBody]Post post)
        {
            if (post == null)
            {
                return BadRequest();
            }
            int id = _posts[_posts.Count - 1].id + 1;
            post.id = id;
            _posts.Add(post);
            this.updateDB();
            return CreatedAtRoute("GetPost", new { id = id }, post);
        }

        // PUT api/posts/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody]Post post)
        {
            if (id != post.id || post == null)
            {
                return BadRequest();
            }
            var postToUpdate = _posts.FirstOrDefault(p => p.id == id);
            if (postToUpdate == null)
            {
                return NotFound();
            }
            postToUpdate.userId = post.userId;
            postToUpdate.title = post.title;
            postToUpdate.body = post.body;
            this.updateDB();
            return Ok(postToUpdate);
        }

        // DELETE api/posts/5
        [HttpDelete("{id}")]
        public IActionResult Remove(int id)
        {
            var postToRemove = _posts.FirstOrDefault(p => p.id == id);
            if (postToRemove == null)
            {
                return NotFound();
            }
            _posts.Remove(postToRemove);
            this.updateDB();
            return new NoContentResult();
        }

        // PATCH api/posts/5
        [HttpPatch("{id}")]
        public IActionResult PartialUpdate(int id, [FromBody]JsonPatchDocument<Post> patch)
        {
            if (patch == null)
            {
                return BadRequest();
            }
            var postToUpdate = _posts.FirstOrDefault(p => p.id == id);
            if (postToUpdate == null)
            {
                return NotFound();
            }
            patch.ApplyTo(postToUpdate, ModelState);
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            this.updateDB();
            return Ok(postToUpdate);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Chatter.Models;
using Newtonsoft.Json;
using Microsoft.AspNet.Identity;

namespace Chatter.Controllers
{
    public class ChatsController : Controller
    {
        private ChatterEntities db = new ChatterEntities();

        // GET: Chats
        public ActionResult Index()
        {
            return View(db.Chats.ToList());
        }
        //string jsonTest = "";
        public JsonResult TestJson()
        {

            var chats = from Chats in db.Chats
                        join AspNetUsers in db.AspNetUsers on new
                        { AspNetUsersID = Chats.AspNetUsersID } equals new
                        { AspNetUsersID = AspNetUsers.Id }
                        orderby
                          Chats.ChatDate
                        select new
                        {
                            AspNetUsers.UserName,
                            Chats.ChatID,
                            Chats.AspNetUsersID,
                            Chats.Message,
                            Chats.ChatDate,
                            Chats.LikeID
                        };
            //string jsonTest = "{ \"firstName\": \"Bob\",\"lastName\": \"Sauce\", " +
            //    "\"children\": [{\"firstName\": \"Barbie\", \"age\": 19 }," +
            //    "{\"firstName\": \"Ron\", \"age\": null }] }";
            var output = JsonConvert.SerializeObject(chats.ToList());

            return Json(output, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult PostChats([Bind(Include = "Message")] Chat chat)
        {
            //we need to fill out information for our model so we can easily insert it.

            //We need the following for our model / db. The user, the message, and a timestamp.
            //Since we're using entity framework, all of the Chat table's fields are in a C# model.
            //We work through that (keeps everything in sync) and the model is what is used to update the database
            //So it's a bit different than adding information directly to the database itself.

            //We already have the message. It was passed to us via AJAX-> JSON vals from the View

            //Let's add the Timestamp field's value
            chat.ChatDate = DateTime.Now;
            //Now, since we have a foreign key join on the aspnetuser table, the 2 models in EF (AspNetUser and Chat) reference each other
            // Because of this, we need to get a complete user object, not just the userID 
            string currentUserId = User.Identity.GetUserId();
            chat.AspNetUsersID = currentUserId;
            chat.AspNetUser = db.AspNetUsers.FirstOrDefault(x => x.Id == currentUserId);
          
           if(ModelState.IsValid)
            {
                db.Chats.Add(chat);
                db.SaveChanges();
               }
            return new JsonResult() { Data = JsonConvert.SerializeObject(chat.ChatID), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        // GET: Chats/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Chat chat = db.Chats.Find(id);
            if (chat == null)
            {
                return HttpNotFound();
            }
            return View(chat);
        }

        // GET: Chats/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Chats/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ChatID,AspNetUsersID,Message,ChatDate,LikeID")] Chat chat)
        {
            
            if (ModelState.IsValid)
            {
                string AspNetUsersId = User.Identity.GetUserId();
                db.Chats.Add(chat);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(chat);
        }

        // GET: Chats/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Chat chat = db.Chats.Find(id);
            if (chat == null)
            {
                return HttpNotFound();
            }
            return View(chat);
        }

        // POST: Chats/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ChatID,AspNetUsersID,Message,ChatDate,LikeID")] Chat chat)
        {
            if (ModelState.IsValid)
            {
                string AspNetUsersID = User.Identity.GetUserId();
                db.Entry(chat).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(chat);
        }

        // GET: Chats/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Chat chat = db.Chats.Find(id);
            if (chat == null)
            {
                return HttpNotFound();
            }
            return View(chat);
        }

        // POST: Chats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Chat chat = db.Chats.Find(id);
            db.Chats.Remove(chat);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

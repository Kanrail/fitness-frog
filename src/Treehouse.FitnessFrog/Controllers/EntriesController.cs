using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Treehouse.FitnessFrog.Data;
using Treehouse.FitnessFrog.Models;

namespace Treehouse.FitnessFrog.Controllers
{
    public class EntriesController : Controller
    {
        private EntriesRepository _entriesRepository = null;

        public EntriesController()
        {
            _entriesRepository = new EntriesRepository();
        }

        public ActionResult Index()
        {
            List<Entry> entries = _entriesRepository.GetEntries();

            // Calculate the total activity.
            double totalActivity = entries
                .Where(e => e.Exclude == false)
                .Sum(e => e.Duration);

            // Determine the number of days that have entries.
            int numberOfActiveDays = entries
                .Select(e => e.Date)
                .Distinct()
                .Count();

            ViewBag.TotalActivity = totalActivity;
            ViewBag.AverageDailyActivity = (totalActivity / (double)numberOfActiveDays);

            return View(entries);
        }

        public ActionResult Add()
        {
            var entry = new Entry()
            {
                Date = DateTime.Today,
            };

            ViewBag.ActivitiesSelectListItems = new SelectList(
                Data.Data.Activities, "Id", "Name");

            return View(entry);
        }

        [HttpPost]
        public ActionResult Add(Entry entry)
        {
            ValidateEntry(entry);

            if (ModelState.IsValid)
            {
                _entriesRepository.AddEntry(entry);

                TempData["Message"] = "Your entry was successfully added!";

                return RedirectToAction("Index");
            }

            SetupActivitiesSelectListItems();

            return View(entry);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //Get the requested entry from the repository
            Entry entry = _entriesRepository.GetEntry((int)id);

            //Return a status of "not found " if entry wasn't found
            if (entry == null)
            {
                return HttpNotFound();
            }

            //Populate the activities select list items ViewBag property.
            SetupActivitiesSelectListItems();

            //Pass entry into the view.
            return View(entry);
        }

        [HttpPost]
        public ActionResult Edit(Entry entry)
        {
            //Validate entry
            ValidateEntry(entry);

            //If entry is valid, use repository to update entry and redirect user to entries list page
            if (ModelState.IsValid)
            {
                _entriesRepository.UpdateEntry(entry);

                TempData["Message"] = "Your entry was successfully updated!";

                return RedirectToAction("Index");
            }

            //Populate activities select list items ViewBag property
            SetupActivitiesSelectListItems();

            return View(entry);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Retrieve entry for provided paremeter value
            Entry entry = _entriesRepository.GetEntry((int) id);

            // Return "not found" if entry wasn't found
            if (entry == null)
            {
                return HttpNotFound();
            }

            // Pass entry to the view
            return View(entry);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            // Delete the entry
            _entriesRepository.DeleteEntry(id);

            TempData["Message"] = "Your entry was successfully deleted!";

            // Redirect user to the "Entries" list page
            return RedirectToAction("Index");
        }

        private void ValidateEntry(Entry entry)
        {
            // If there aren't any "Duration" field validation errors, then make sure that
            // the duration is greater than 0.
            if (ModelState.IsValidField("Duration") && entry.Duration <= 0)
            {
                ModelState.AddModelError("Duration", "The Duration field value must be greater than '0'.");
            }
        }

        private void SetupActivitiesSelectListItems()
        {
            ViewBag.ActivitiesSelectListItems = new SelectList(
                Data.Data.Activities, "Id", "Name");
        }
    }
}
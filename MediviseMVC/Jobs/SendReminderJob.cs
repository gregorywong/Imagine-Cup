﻿/*
 * Team Name: EOS
 * Team Memebers: Jason Cheng, Gregory Wong, Xihan Zhang, Wenshiang Chung
 * E-mail: eos_imaginecup@hotmail.com
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MediviseMVC.Models;
using Quartz;
using MediviseMVC.Twilio;
using System.Diagnostics;
using System.Data;
namespace MediviseMVC.Jobs
{
    public class SendReminderJob : IJob
    {
        private TwilioSender sender;
        private MediviseEntities db;
        private MessageConstructor msgBuilder = new MessageConstructor();
        private int reminderHour = 14; //send reminders at 14:00 or 2pm
        public void Execute (IJobExecutionContext context)
        {
            sender = new TwilioSender();
            db = new MediviseEntities();
            sendReminders();
        }
        private bool IsDrugNeededToday(Drug d)
        {
            if (DateTime.UtcNow.DayOfWeek.Equals(DayOfWeek.Monday) && d.Monday)
            {
                return true;
            }
            else if (DateTime.UtcNow.DayOfWeek.Equals(DayOfWeek.Tuesday) && d.Tuesday)
            {
                return true;
            }
            else if (DateTime.UtcNow.DayOfWeek.Equals(DayOfWeek.Wednesday) && d.Wednesday)
            {
                return true;
            }
            else if (DateTime.UtcNow.DayOfWeek.Equals(DayOfWeek.Thursday) && d.Thursday)
            {
                return true;
            }
            else if (DateTime.UtcNow.DayOfWeek.Equals(DayOfWeek.Friday) && d.Friday)
            {
                return true;
            }
            else if (DateTime.UtcNow.DayOfWeek.Equals(DayOfWeek.Saturday) && d.Saturday)
            {
                return true;
            }
            else if (DateTime.UtcNow.DayOfWeek.Equals(DayOfWeek.Sunday) && d.Sunday)
            {
                return true;
            }
            return false;
        }
        private List<Drug> DrugsNeededToday(Patient p)
        {
            List<Drug> drugsToday = new List<Drug>();
            List<Drug> drugs = db.Drugs.Where(d => d.PatientId == p.PatientId).ToList();
            foreach (Drug d in drugs)
            {
                if (IsDrugNeededToday(d))
                {
                    drugsToday.Add(d);
                }
            }
            return drugsToday;

        }
        private void sendReminders()
        {
            var patients = db.Patients.ToArray();
            foreach (Patient p in patients)
            {
                //see what time it is in user TimeZone
                TimeZoneInfo userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(p.TimeZone);
                DateTime timeInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, userTimeZone);

                if (timeInUserTimeZone.Hour == reminderHour)
                {
                    if (treatmentCompleted(p))
                    {
                        messageIfCompletedToday(p);
                    }
                    else
                    {
                        List<Drug> drugsToday = DrugsNeededToday(p);
                        if (drugsToday.Count > 0) //treatment needed today
                        {
                            if (p.ResponseReceived == false)
                            {
                                string warning = "You forgot yesterday's medication!";
                                sender.SendSMS(p.Phone, warning);
                                constructAlert(p);
                            }
                            p.ResponseReceived = false;
                            db.Entry(p).State = EntityState.Modified;
                            db.SaveChanges();
                            string message = msgBuilder.ConstructMsg(p, drugsToday.Count);
                            Trace.WriteLine(p.Phone, "PHONE NUMBER ********************");
                            sender.SendSMS(p.Phone, message);
                        }
                        else //treatment not needed today
                        {
                            if (p.ResponseReceived == false)
                            {
                                string warning = "You forgot yesterday's medication!";
                                sender.SendSMS(p.Phone, warning);
                                constructAlert(p);
                                p.ResponseReceived = true; //so we don't penalize them again
                                db.Entry(p).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            string message = msgBuilder.ConstructMsg(p, drugsToday.Count);
                            Trace.WriteLine(p.Phone, "PHONE NUMBER ********************");
                            sender.SendSMS(p.Phone, message);
                        }
                    }
                }
            }
        }
        private bool treatmentCompleted(Patient p)
        {
            return (DateTime.UtcNow >= p.TreatmentEndDate.AddDays(1));
        }
        private void messageIfCompletedToday(Patient p) //send message on day of completion
        {
            if (DateTime.UtcNow.Date == p.TreatmentEndDate.AddDays(1).Date)
            {
                string congrats = "Congratulations! You have successfully completed the treatment!";
                Trace.WriteLine(p.Phone, "PHONE NUMBER ********************");
                sender.SendSMS(p.Phone, congrats);
                completionAlert(p);
            }
        }
        private void constructAlert(Patient p)
        {
            Alert a = new Alert
            {
                PatientId = p.PatientId,
                AlertDate = DateTime.UtcNow.AddDays(-1),
                AlertTypeId = 1 // 1 = "missed medication"
            };
            db.Alerts.Add(a);
            db.SaveChanges();
        }
        private void completionAlert(Patient p)
        {
            Alert a = new Alert
            {
                PatientId = p.PatientId,
                AlertDate = DateTime.UtcNow,
                AlertTypeId = 3 // 3 = "Completed Treatment"
            };
            db.Alerts.Add(a);
            db.SaveChanges();
        }


    }
}
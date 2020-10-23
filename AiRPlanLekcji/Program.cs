using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

using System.Threading;
using System.Collections.Generic;
using System.Globalization;

namespace AiRPlanLekcji
{
    class Program
    {
        struct Lecture
        {
            DateTime start;
            DateTime end;
            string subject;
            string lecturer;
            Types type;

            enum Types
            {
                Nieznany,
                Wykład,
                ĆwiczeniaLabolatoryjne,
                ĆwiczeniaAudytoryjne,
                Inne
            }

            string TypeToString(Types type)
            {
                switch (type)
                {
                    case Types.Nieznany:
                        return "Nieznany";
                    case Types.Wykład:
                        return "Wykład";
                    case Types.ĆwiczeniaLabolatoryjne:
                        return "Ćwiczenia Labolatoryjne";
                    case Types.ĆwiczeniaAudytoryjne:
                        return "Ćwiczenia Audytoryjne";
                    case Types.Inne:
                        return "Inne";
                }

                return "";
            }

            public Lecture(DateTime date, string timeData, string data)
            {
                start = DateTime.MinValue;
                end = DateTime.MinValue;
                subject = "";
                lecturer = "";
                type = Types.Nieznany;

                Parse(date, timeData, data);
            }

            Types StrToType(string data)
            {
                Console.WriteLine("\"" + data + "\"");

                switch (data)
                {
                    case "Ćwicz. aud":
                        return Types.ĆwiczeniaAudytoryjne;

                    case "Ćwicz. lab":
                        return Types.ĆwiczeniaLabolatoryjne;

                    case "ćwiczenia laboratoryjne":
                        return Types.ĆwiczeniaLabolatoryjne;

                    case "Wykład":
                        return Types.Wykład;

                    case "Inne":
                        return Types.Inne;

                    default:
                        return Types.Nieznany;
                }
            }
            
            public void Parse(DateTime date, string timeData, string data)
            {
                data = data.Replace("\r", "");

                string[] ts = timeData.Split(" - ");
                TimeSpan st = TimeSpan.ParseExact(ts[0], "h\\:mm", CultureInfo.CurrentCulture);
                start = new DateTime(date.Year, date.Month, date.Day, st.Hours, st.Minutes, 0);

                st = TimeSpan.ParseExact(ts[1], "h\\:m", CultureInfo.CurrentCulture);
                end = new DateTime(date.Year, date.Month, date.Day, st.Hours, st.Minutes, 0);


                // 1. {przedmiot}, {typ}, {grupa}
                // 2. , prowadzący: {prowadzący}, {tytuł}
                // 3* Infomacja: {sposób prowadzenia}
                string[] lines = data.Split('\n');
                var line1 = lines[0].Split(", ");
                subject = line1[0];
                type = StrToType(line1[1]);
            }

            public override string ToString()
            {
                return "[" + start.Date.ToString("dddd d.M.yyyy") + "] " + subject + "\n\t- " + start.TimeOfDay.ToString("h\\:mm") + " - " + end.TimeOfDay.ToString("h\\:mm") + "\n\t- " + TypeToString(type);
            }
        }

        static void Main(string[] args)
        {
            using(ChromeDriver driver = new ChromeDriver())
            {
                driver.Url = "https://planzajec.eaiib.agh.edu.pl/view/timetable/689";
                driver.Navigate();

                var groupSelect = driver.FindElementByClassName("fc-groupFilter-button");
                groupSelect.Click();

                char grupa = '5';
                char podGrupa = 'b';

                //groupSelect.FindElement(By.ClassName(""))
                var dropdown = driver.FindElementsByClassName("dropdown-menu")[1];
                //var element = dropdown.FindElements(By.TagName("li"))[numer];
                var elements = dropdown.FindElements(By.TagName("li"));

                //element.Click();

                for(int i = 0;i<elements.Count;i++)
                {
                    if(elements[i].Text[0] == grupa)
                    {
                        elements[i].Click();
                        break;
                    }
                }

                var plan = driver.FindElementByClassName("fc-content-skeleton");
                var days = plan.FindElements(By.TagName("td"));

                string[] names = new string[]{ "hours", "Poniedziałek", "Wtorek", "Środa", "Czwartek", "Piątek" };

                List<Lecture> lectures = new List<Lecture>();
                // Skip hours column
                DateTime day = DateTime.Now.AddDays(-1 * (int)DateTime.Now.DayOfWeek + 1);
                //Console.WriteLine("Week start: " + weekStart.ToString());
                for (int i = 1;i<days.Count;i++)
                {
                    Console.WriteLine(names[i] + ":");
                    var lessonsGrid = days[i].FindElements(By.ClassName("fc-time-grid-event"));
                    for(int j = 0;j< lessonsGrid.Count;j++)
                    {
                        var time = lessonsGrid[j].FindElement(By.ClassName("fc-time"));
                        //Console.WriteLine("\tTime: " + time.GetAttribute("data-full"));

                        var title = lessonsGrid[j].FindElement(By.ClassName("fc-title"));
                        //Console.WriteLine("\tText: " + title.Text);

                        

                        lectures.Add(new Lecture(day, time.GetAttribute("data-full"), title.Text));
                    }
                    day = day.AddDays(1);
                }

                for(int i = 0;i<lectures.Count;i++)
                {
                    Console.WriteLine(lectures[i].ToString() + "\n");
                }

                Console.ReadKey();
            }
        }
    }
}

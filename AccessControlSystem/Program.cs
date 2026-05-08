using System;
using Spectre.Console;
using AccessControlSystem.Data;
using AccessControlSystem.Controllers;
using AccessControlSystem.Listeners;
using AccessControlSystem.Models;
using System.Data;

namespace AccessControlSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var db = new DatabaseManager();
            var controller = new AccessController();
            var dbLogger = new DatabaseLogger(db);

            // Абониране на БД логъра
            controller.AccessGranted += dbLogger.LogSuccessToDb;
            controller.AccessDenied += dbLogger.LogFailureToDb;

            while (true)
            {
                Console.Clear();
                AnsiConsole.Write(new FigletText("Secure Access").Centered().Color(Color.Aqua));
                AnsiConsole.Write(new Spectre.Console.Rule("[yellow]ПАНЕЛ ЗА УПРАВЛЕНИЕ[/]").Centered());

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                    .Title("Изберете опция:")
                    .AddChoices(new[] {
                        "🔑 Сканирай карта",
                        "👥 Списък Потребители & Карти",
                        "🚪 Списък Врати & Права",   
                        "➕ Добави нов Потребител",
                        "🧱 Добави нова Врата",
                        "🗑️ ИЗТРИЙ Потребител",
                        "🗑️ ИЗТРИЙ Врата",      
                        "📊 LINQ: Последни 10 събития",
                        "💾 Експорт в CSV",
                        "❌ Изход"
                    }));

                switch (choice)
                {
                    case "🔑 Сканирай карта":
                        ScanCardFlow(db, controller);
                        break;

                    case "👥 Списък Потребители & Карти":
                        ShowUsersTable(db);
                        break;

                    case "🚪 Списък Врати & Права":
                        ShowDoorsTable(db);
                        break;

                    case "➕ Добави нов Потребител":
                        AddUserFlow(db);
                        break;

                    case "🧱 Добави нова Врата":
                        AddDoorFlow(db);
                        break;

                    case "📊 LINQ: Последни 10 събития":
                        db.PrintLast10Events(); // Увери се, че методът ползва AnsiConsole за красота
                        break;

                    case "💾 Експорт в CSV":
                        new FileManager().ExportLogsToCsv(db.GetAllLogs());
                        break;

                    case "🗑️ ИЗТРИЙ Потребител":
                        DeleteUserFlow(db);
                        break;

                    case "🗑️ ИЗТРИЙ Врата":
                        DeleteDoorFlow(db);
                        break;
                    case "❌ Изход":
                        return;
                }
                AnsiConsole.MarkupLine("\n[grey]Натиснете Enter за меню...[/]");
                Console.ReadLine();
            }
        }

        // --- FLOW МЕТОДИ С ВАЛИДАЦИЯ ---

        static void ScanCardFlow(DatabaseManager db, AccessController controller)
        {
            var cardNum = AnsiConsole.Ask<string>("💳 [yellow]Номер на карта:[/]");
            var roomName = AnsiConsole.Ask<string>("🚪 [yellow]Име на стая:[/]");

            var card = db.GetCardFromDb(cardNum);
            var door = db.GetDoorFromDb(roomName);

            if (door == null)
            {
                AnsiConsole.MarkupLine("[red]❌ ГРЕШКА: Вратата не съществува в базата данни![/]");
                return;
            }

            // Ръчно добавяме в контролера само за тази проверка (Loose Coupling)
            controller.RegisterDoor(door);
            if (card != null) controller.RegisterCard(card);

            controller.TryAccess(cardNum, roomName);
        }

        static void AddUserFlow(DatabaseManager db)
        {
            string fn = AnsiConsole.Prompt(new TextPrompt<string>("Име:").Validate(s => !string.IsNullOrWhiteSpace(s)));
            string ln = AnsiConsole.Prompt(new TextPrompt<string>("Фамилия:").Validate(s => !string.IsNullOrWhiteSpace(s)));

            string card = AnsiConsole.Prompt(new TextPrompt<string>("Номер на нова карта:")
                .Validate(c => {
                    if (db.DoesCardExist(c)) return ValidationResult.Error("[red]Тази карта вече е заета![/]");
                    return ValidationResult.Success();
                }));

            int role = AnsiConsole.Prompt(new SelectionPrompt<int>().Title("Изберете роля:").AddChoices(0, 1, 2, 3));

            db.AddUserWithCard(fn, ln, role, card);
            AnsiConsole.MarkupLine("[green]✅ Потребителят е създаден успешно![/]");
        }

        static void AddDoorFlow(DatabaseManager db)
        {
            string name = AnsiConsole.Prompt(new TextPrompt<string>("Име на помещение:")
                .Validate(n => {
                    if (db.DoesDoorExist(n)) return ValidationResult.Error("[red]Този кабинет вече съществува![/]");
                    return ValidationResult.Success();
                }));

            int role = AnsiConsole.Prompt(new SelectionPrompt<int>().Title("Минимална роля за достъп:").AddChoices(0, 1, 2, 3));

            db.AddDoor(name, role);
            AnsiConsole.MarkupLine("[green]✅ Вратата е регистрирана![/]");
        }

        // --- КРАСИВИ ТАБЛИЦИ ---

        static void ShowUsersTable(DatabaseManager db)
        {
            var table = new Table().Border(TableBorder.Rounded).Title("[blue]РЕГИСТРИРАНИ ПОТРЕБИТЕЛИ[/]");
            table.AddColumn("Име"); table.AddColumn("Фамилия"); table.AddColumn("Роля"); table.AddColumn("Карта"); table.AddColumn("Статус");

            DataTable dt = db.GetAllUsersWithCards();
            foreach (DataRow row in dt.Rows)
            {
                table.AddRow(row[0].ToString(), row[1].ToString(), ((Role)row[2]).ToString(),
                             $"[yellow]{row[3]}[/]", (bool)row[4] ? "[green]Активна[/]" : "[red]Блокирана[/]");
            }
            AnsiConsole.Write(table);
        }

        static void ShowDoorsTable(DatabaseManager db)
        {
            var table = new Table().Border(TableBorder.DoubleEdge).Title("[magenta]КОНТРОЛИРАНИ ОБЕКТИ[/]");
            table.AddColumn("Помещение"); table.AddColumn("Изисквана Роля");

            DataTable dt = db.GetAllDoors();
            foreach (DataRow row in dt.Rows)
            {
                table.AddRow(row[0].ToString(), $"[bold]{(Role)row[1]}[/]");
            }
            AnsiConsole.Write(table);
        }


        static void DeleteUserFlow(DatabaseManager db)
        {
            // 1. Вземаме всички потребители за списък
            DataTable dt = db.GetAllUsersWithCards();
            if (dt.Rows.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Няма регистрирани потребители за изтриване.[/]");
                return;
            }

            var userMap = new System.Collections.Generic.Dictionary<string, string>();
            foreach (DataRow row in dt.Rows)
            {
                string displayName = $"{row[0]} {row[1]} (Карта: {row[3]})";
                userMap[displayName] = row[3].ToString(); // Пазим CardNumber
            }

            // 2. Потребителят избира от списък
            var selectedUser = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[red]Изберете потребител за ИЗТРИВАНЕ:[/]")
                    .AddChoices(userMap.Keys));

            // 3. Потвърждение (Safety check)
            if (AnsiConsole.Confirm($"Сигурни ли сте, че искате да изтриете {selectedUser}?"))
            {
                db.DeleteUserByCard(userMap[selectedUser]);
                AnsiConsole.MarkupLine("[green]✅ Потребителят и неговата карта бяха изтрити успешно![/]");
            }
        }

        static void DeleteDoorFlow(DatabaseManager db)
        {
            // 1. Вземаме всички врати
            DataTable dt = db.GetAllDoors();
            if (dt.Rows.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Няма регистрирани врати.[/]");
                return;
            }

            var doorList = new System.Collections.Generic.List<string>();
            foreach (DataRow row in dt.Rows)
            {
                doorList.Add(row[0].ToString());
            }

            // 2. Избор от списък
            var selectedDoor = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[red]Изберете врата за ПРЕМАХВАНЕ:[/]")
                    .AddChoices(doorList));

            // 3. Потвърждение
            if (AnsiConsole.Confirm($"Сигурни ли сте, че искате да премахнете врата '{selectedDoor}'?"))
            {
                db.DeleteDoor(selectedDoor);
                AnsiConsole.MarkupLine("[green]✅ Вратата беше премахната от системата![/]");
            }
        }

    }
}
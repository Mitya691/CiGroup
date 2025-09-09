using ClosedXML.Excel;
using DesktopClient.Helpers;
using DesktopClient.Model;
using MailKit.Net.Smtp;
using MimeKit;
using System.Globalization;
using System.IO;

namespace DesktopClient.Services
{
    /// <summary>
    /// Реализация сервиса создания отчета в форматет xlsx 
    /// SDK NanoXLSX
    /// </summary>
    class ReportService : IReportService
    {
        private readonly CurrentUserStore _currentUser;
        private readonly ISQLRepository _repository;
        private readonly string _person;

        // внедрение зависимости через конструктор
        public ReportService(ISQLRepository repository, CurrentUserStore currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<string> NewReport(DateTime? Start, DateTime? Stop)
        {
            List<Card> cards = await _repository.GetCardsForInterval(Start, Stop);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.AddWorksheet("Отчёт");

            worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;

            worksheet.Range("A1 : B1").Merge(); //Организация
            worksheet.Range("A2 : B2").Merge(); //Подразделение

            worksheet.Range("D1 : E1").Merge(); //ООО МакПром
            worksheet.Range("D2 : E2").Merge(); //МиПЭ

            worksheet.Range("B3 : J3").Merge(); //Название листа

            worksheet.Range("F4 : G4").Merge(); //ФИО оператора

            worksheet.Range("A8 : A9").Merge(); //Номер операции
            worksheet.Range("B8 : D8").Merge(); //Перемещение зерна
            worksheet.Range("E8 : F8").Merge(); //Время перемещения
            worksheet.Range("G8 : H8").Merge(); //Показания весов
            worksheet.Range("I8 : I9").Merge(); //Перемещено за операцию

            //Шапка листа
            worksheet.Cell("A1").Value = "Организация:";
            worksheet.Cell("A2").Value = "Подразделение:";
            worksheet.Cell("D1").Value = "ООО \"МакПром\"";
            worksheet.Cell("D1").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            worksheet.Cell("A2").Value = "МиПЭ";
            worksheet.Cell("D1").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            
            worksheet.Cell("B3").Value = "Отчет перемещения зерна с элеваторных сооружений на мельницы";

            worksheet.Cell("A4").Value = "Дата:";
            worksheet.Cell("A5").Value = "Время начала смены:";
            worksheet.Cell("A6").Value = "Время окончания смены:";
            worksheet.Cell("E4").Value = "Смена:";

            var date = worksheet.Cell("B4");
            var shiftStart = worksheet.Cell("E5");
            var shiftEnd = worksheet.Cell("E6");
            var person = worksheet.Cell("F4");


            date.Style.Border.OutsideBorder = XLBorderStyleValues.Double;
            shiftStart.Style.Border.OutsideBorder = XLBorderStyleValues.Double;
            shiftEnd.Style.Border.OutsideBorder = XLBorderStyleValues.Double;
            person.Style.Border.OutsideBorder = XLBorderStyleValues.Double;

            //Шапка таблицы
            worksheet.Cell("A8").Value = "№ операции";
            worksheet.Cell("B8").Value = "Перемещение зерна";
            worksheet.Cell("E8").Value = "Время перемещения";
            worksheet.Cell("G8").Value = "Показания весов";
            worksheet.Cell("I8").Value = "Перемещено за операцию";

            worksheet.Cell("B9").Value = "Из силоса элеватора";
            worksheet.Cell("C9").Value = "В мельницу";
            worksheet.Cell("D9").Value = "Силос мельницы";
            worksheet.Cell("E9").Value = "Начало";
            worksheet.Cell("F9").Value = "Окончание";
            worksheet.Cell("G9").Value = "Весы 1";
            worksheet.Cell("H9").Value = "Весы 2";

            int rowCounter = 10;
            int operationNumber = 1;

            decimal? WeightM1 = 0;
            decimal? WeightM2 = 0;
            decimal? totalWeight = 0;

            date.Value = DateTime.Now.ToString("dd.MM.yyyy");
            shiftStart.Value = Start;
            shiftEnd.Value = Stop;
            person.Value = _currentUser.CurrentUser?.FullName;

            foreach (Card card in cards)
            {
                worksheet.Cell(rowCounter, 1).Value = operationNumber;
                worksheet.Cell(rowCounter, 2).Value = card.SourceSilo;
                worksheet.Cell(rowCounter, 3).Value = card.Direction;
                worksheet.Cell(rowCounter, 4).Value = card.TargetSilo;
                worksheet.Cell(rowCounter, 5).Value = card.StartTime;
                worksheet.Cell(rowCounter, 6).Value = card.EndTime;
                worksheet.Cell(rowCounter, 7).Value = card.Weight1;
                worksheet.Cell(rowCounter, 8).Value = card.Weight2;
                worksheet.Cell(rowCounter, 9).Value = card.TotalWeight;

                if (card.Direction == "М1") 
                {
                    WeightM1 += card.TotalWeight;
                }
                else if (card.Direction == "М2")
                {
                    WeightM2 += card.TotalWeight;
                }

                totalWeight += card.TotalWeight;

                rowCounter++;
                operationNumber++;
            }

            var range = worksheet.Range(1, 8, rowCounter, 9);
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            //Итоговые значения в конце таблицы
            worksheet.Range(rowCounter, 2, rowCounter, 6);
            worksheet.Cell(rowCounter, 2).Value = "Итого суммарно на конец смены:  ";

            worksheet.Range(rowCounter, 7, rowCounter, 8);
            worksheet.Cell(rowCounter, 7).Value = totalWeight;
            worksheet.Cell(rowCounter, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Double;

            worksheet.Cell(rowCounter, 9).Value = "тонн";

            rowCounter++;

            worksheet.Range(rowCounter, 2, rowCounter, 6);
            worksheet.Cell(rowCounter, 2).Value = "В том числе на мельницу 1:  ";

            worksheet.Range(rowCounter, 7, rowCounter, 8);
            worksheet.Cell(rowCounter, 7).Value = WeightM1;
            worksheet.Cell(rowCounter, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Double;

            worksheet.Cell(rowCounter, 9).Value = "тонн";

            rowCounter++;

            worksheet.Range(rowCounter, 2, rowCounter, 6);
            worksheet.Cell(rowCounter, 2).Value = "В том числе на мельницу 2:  ";

            worksheet.Range(rowCounter, 7, rowCounter, 8);
            worksheet.Cell(rowCounter, 7).Value = totalWeight;
            worksheet.Cell(rowCounter, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Double;

            worksheet.Cell(rowCounter, 9).Value = "тонн";

            rowCounter += 2;

            worksheet.Cell(rowCounter, 6).Value = "Оператор ПУЭ";
            worksheet.Range(rowCounter, 8, rowCounter, 9);
            worksheet.Cell(rowCounter, 8).Style.Border.BottomBorder = XLBorderStyleValues.Thin;

            worksheet.Cell(rowCounter, 10).Value = _currentUser.CurrentUser?.FullName;

            string filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            $"Report_{DateTime.Now:yyyy_MM_dd_HHmm}.xlsx");

            workbook.SaveAs(filePath);

            return filePath;
        }

        public async void SendReport(string reportPath, string mail)
        {
            string header = $"Отчёт о  {DateTime.Now.Date.Subtract(TimeSpan.FromDays(1)).ToString("d", CultureInfo.CurrentCulture)}";

            MimeMessage message = new MimeMessage();
           
            message.Subject = header;

            string messageText = "ООО \"МакПром\"" +
                                 $"{header}\n" +
                                 "\n" +
                                 "Отчёт находится в прикреплённом к письму файле.\n" +
                                 "Пожалуйста, не отвечайте на это сообщение.\n" +
                                 "\n" +
                                 "Служба автоматической отправки сообщений ООО \"МакПром\"\n" +
                                 "Тел. для справок: +7-(961)-671-41-45";
            TextPart body = new TextPart("plain") { Text = messageText };
            Multipart multipart = new Multipart("mixed");
            multipart.Add(body);

            MimePart attachment = new MimePart("application", "vnd.ms-excel")
            {
                Content = new MimeContent(File.OpenRead(reportPath), ContentEncoding.Default),
                ContentDisposition = new MimeKit.ContentDisposition(MimeKit.ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = Path.GetFileName(reportPath)
            };

            //замена кодировки чтобы имя вложенного в письмо файла читалось корректно
            foreach (Parameter parameter in attachment.ContentType.Parameters) parameter.EncodingMethod = ParameterEncodingMethod.Rfc2047;
            foreach (Parameter parameter in attachment.ContentDisposition.Parameters) parameter.EncodingMethod = ParameterEncodingMethod.Rfc2047;

            multipart.Add(attachment);
            message.Body = multipart;

            SmtpClient client = new SmtpClient();
            try
            {
                client.ConnectAsync("smtp.yandex.ru", 465, true);
                client.AuthenticateAsync("your-mail", "the password issued by mail to your application");
                client.SendAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.DisconnectAsync(true);
                client.Dispose();
            }

        }
    }
}

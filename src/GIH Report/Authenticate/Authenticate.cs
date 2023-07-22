using GIH_Report.Constant;
using System.Net.Http.Headers;

namespace GIH_Report.Authenticate
{
    public class Authenticate
    {
        /// <summary>
        /// Authenticate a user
        /// </summary>
        /// <returns></returns>
        public static HttpClient AuthenticateUser()
        {
            Console.Write("\n Jira username: ");
            string username = Console.ReadLine();
            Console.Write("\n Jira password: ");
            var pass = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    Console.Write("\b \b");
                    pass = pass[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    pass += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);
            var client = new HttpClient();
            client.BaseAddress = new Uri(Constants.baseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{username}:{pass}")));
            return client;
        }

    }
}

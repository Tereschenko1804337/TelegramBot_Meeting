using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Args;    //Для получения типа сообщений
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using System.Net;
using System.Data;
using MySql.Data.MySqlClient;


//TODO: собирать id сообщений в массив для их нормального удаления

namespace TelegramBot
{
    class Program
    {
        static string search_users(int chat_id)
        {
            //bool search = false;
            MySqlConnection conn;
            string connStr =
                @"Server = сервер; Database = база; Uid = логин; Pwd = Пароль; charset=utf8";
            conn = new MySqlConnection(connStr);

            conn.Open();
            string SELECT_SEARCH_USERS = "SELECT chat_id, User_Name, User_Account_Name, hobby1, hobby2, hobby3, hobby4, hobby5 FROM table_info_about_users";
            MySqlCommand COMMAND_SELECT_SEARCH_USERS = new MySqlCommand(SELECT_SEARCH_USERS, conn);
            var answer = COMMAND_SELECT_SEARCH_USERS.ExecuteReader();

            var lists = new List<object[]>();
            var people_who_find = new List<object[]>();

            while (answer.Read())
            {
                var arr = new object[8];
                arr[0] = answer[0];
                arr[1] = answer[1];
                arr[2] = answer[2];
                arr[3] = answer[3];
                arr[4] = answer[4];
                arr[5] = answer[5];
                arr[6] = answer[6];
                arr[7] = answer[7];

                if (Convert.ToInt32(arr[0]) == chat_id)
                    people_who_find.Add(arr);
                else
                    lists.Add(arr);
            }
            conn.Close();

            var peoples = new List<object[]>();
            var haracters = new object[3];
            string message_answer = "";
            int count = 0;

            for (int i = 3; i < 8; i++)
            {
                for (int j = 0; j < lists.Count(); j++)
                {
                    for (int k = 3; k < 8; k++)
                    {
                        if (Convert.ToString(people_who_find[0][i]) == Convert.ToString(lists[j][k]))
                            count += 1;

                        if (count >= 2)
                        {
                            message_answer += $"Имя: {lists[j][2]}; В телеграме: {lists[j][1]} \n";
                            count = 0;
                            continue;
                        }
                    }
                }
            }


            return message_answer;
        }


        // false для добавления данных
        // true для изменения данных
        static string data_base(bool update_data, string users_message_hobbys, int chat_id) 
        {
            // программирование, рисование, музыка, дизайн, путешествия
            string[] hobbys = users_message_hobbys.ToLower().Replace(" ", "").Split(new Char[] { ',' });

            MySqlConnection conn;
            string connStr =
                @"Server = сервер; Database = база; Uid = логин; Pwd = Пароль; charset=utf8";
            conn = new MySqlConnection(connStr);

            conn.Open();

            if (update_data == true)
            {
                string UPDATE_HOBBYS = "UPDATE table_info_about_users SET hobby1 = @hobby1, hobby2 = @hobby2, hobby3 = @hobby3, hobby4 = @hobby4, hobby5 = @hobby5 " +
                    "WHERE chat_id = @chat_id";
                MySqlCommand COMMAND_UPDATE_HOBBYS = new MySqlCommand(UPDATE_HOBBYS, conn);
                COMMAND_UPDATE_HOBBYS.Parameters.AddWithValue("chat_id", chat_id);
                try
                {
                    COMMAND_UPDATE_HOBBYS.Parameters.AddWithValue("hobby1", hobbys[0]);
                    COMMAND_UPDATE_HOBBYS.Parameters.AddWithValue("hobby2", hobbys[1]);
                    COMMAND_UPDATE_HOBBYS.Parameters.AddWithValue("hobby3", hobbys[2]);
                    COMMAND_UPDATE_HOBBYS.Parameters.AddWithValue("hobby4", hobbys[3]);
                    COMMAND_UPDATE_HOBBYS.Parameters.AddWithValue("hobby5", hobbys[4]);
                    COMMAND_UPDATE_HOBBYS.ExecuteNonQuery();
                }
                catch
                {
                    conn.Close();
                    return "Введите 5 значений!";
                }
            }
            else
            {
                string INSERT_HOBBYS = "UPDATE table_info_about_users SET hobby1 = @hobby1, hobby2 = @hobby2, hobby3 = @hobby3, hobby4 = @hobby4, hobby5 = @hobby5 " +
                    "WHERE chat_id = @chat_id";//"INSERT INTO table_info_about_users (hobby1, hobby2, hobby3, hobby4, hobby5) VALUE (@hobby1, @hobby2, @hobby3, @hobby4, @hobby5) WHERE chat_id = @chat_id";
                MySqlCommand COMMAND_INSERT_HOBBYS = new MySqlCommand(INSERT_HOBBYS, conn);
                COMMAND_INSERT_HOBBYS.Parameters.AddWithValue("chat_id", chat_id);
                //try
                //{
                    COMMAND_INSERT_HOBBYS.Parameters.AddWithValue("hobby1", hobbys[0]);
                    COMMAND_INSERT_HOBBYS.Parameters.AddWithValue("hobby2", hobbys[1]);
                    COMMAND_INSERT_HOBBYS.Parameters.AddWithValue("hobby3", hobbys[2]);
                    COMMAND_INSERT_HOBBYS.Parameters.AddWithValue("hobby4", hobbys[3]);
                    COMMAND_INSERT_HOBBYS.Parameters.AddWithValue("hobby5", hobbys[4]);
                    COMMAND_INSERT_HOBBYS.ExecuteNonQuery();
                //}
                //catch
                //{
                //    conn.Close();
                //    return "Введите 5 значений!";
                //}
            }

            //bool check_user = dataReader_SELECT_CHECK_USER.Read();
            conn.Close();
            return "";
        }


        // Метод проверяет есть ли пользователь в базе. Если нет, то заносит
        // Так же проверяет есть ли у пользователя записанные хобби
        static void check_dilog(int chat_id, string User_Account_Name, string User_Name)
        {
            MySqlConnection conn;
            string connStr = @"Server = сервер; Database = база; Uid = логин; Pwd = Пароль; charset=utf8";
                
            conn = new MySqlConnection(connStr);

            conn.Open();
                string SELECT_CHECK_USER = "SELECT chat_id FROM table_info_about_users WHERE chat_id = @chat_id";
                MySqlCommand COMMAND_SELECT_CHECK_USER = new MySqlCommand(SELECT_CHECK_USER, conn);
                COMMAND_SELECT_CHECK_USER.Parameters.AddWithValue("chat_id", chat_id);
                MySqlDataReader dataReader_SELECT_CHECK_USER = COMMAND_SELECT_CHECK_USER.ExecuteReader();

                bool check_user = dataReader_SELECT_CHECK_USER.Read();
            conn.Close();


            if (check_user == false)    // Если пользователя нет, то добавляем его
            {
                conn.Open();
                    string INSERT_NEW_USER = "INSERT INTO table_info_about_users (chat_id, User_Account_Name, User_Name) VALUES (@chat_id, @User_Account_Name, @User_Name)";
                    MySqlCommand COMMAND_INSERT_NEW_USER = new MySqlCommand(INSERT_NEW_USER, conn);
                    COMMAND_INSERT_NEW_USER.Parameters.AddWithValue("chat_id", chat_id);
                    COMMAND_INSERT_NEW_USER.Parameters.AddWithValue("User_Account_Name", User_Account_Name);
                    COMMAND_INSERT_NEW_USER.Parameters.AddWithValue("User_Name", User_Name);
                    COMMAND_INSERT_NEW_USER.ExecuteNonQuery();
                conn.Close();
            }


            // Проверка есть ли уже хобби у человека, для изменения кнопки "Добавить/изменить хобби"
            conn.Open();
                string SELECT_CHECK_HOBBYS = "SELECT * FROM table_info_about_users WHERE chat_id = @chat_id " +
                    "AND hobby1 != '' " +
                    "AND hobby2 != '' " +
                    "AND hobby3 != '' " +
                    "AND hobby4 != '' " +
                    "AND hobby5 != '' ";
                MySqlCommand COMMAND_SELECT_CHECK_HOBBYS = new MySqlCommand(SELECT_CHECK_HOBBYS, conn);
                COMMAND_SELECT_CHECK_HOBBYS.Parameters.AddWithValue("chat_id", chat_id);
                MySqlDataReader dataReader_SELECT_CHECK_HOBBYS = COMMAND_SELECT_CHECK_HOBBYS.ExecuteReader();

                bool check_hobbys = dataReader_SELECT_CHECK_HOBBYS.Read();
            conn.Close();

            if (check_hobbys == true)
            {
                users_autorization = false;
            }
            else
            {
                check_method = false;
                users_autorization = true;
            }
            //check_method = false;

        }

        static TelegramBotClient Bot;

        static string Users_answer = ""; //Для получения последнего сообщения от пользователя
        static bool users_autorization = false; //Для понимания, есть ли пользователь в БД
                                                //Он либо может изменить данные, если False
                                                //Он может записать данные, если True
        static bool check_method = true;    // Для одной проверки на авторизацию
        static int message_id;  //Получаем Id сообщения, отправляемого ботом, для его удаления
        static string number_button_info = "";

        //Подключаемся к боту и прослушиваем его
        static void Main(string[] args)
        {
            try
            {
                
                Bot = new TelegramBotClient("ваш токен");

                Bot.OnMessage += BotOnMessageReceived;

                Bot.OnCallbackQuery += BotOnCallBackQueryReceived;


                var me = Bot.GetMeAsync().Result;

                Console.WriteLine(me.FirstName);    //Вывод в консоль имени бота

                Bot.StartReceiving();
                Console.ReadLine();
                Bot.StopReceiving();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
        }

        private static async void BotOnCallBackQueryReceived(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            string buttonText = e.CallbackQuery.Data;
            string user_name = $"{e.CallbackQuery.From.FirstName} {e.CallbackQuery.From.Id}";
            Users_answer = buttonText;

            Console.WriteLine($"{user_name} нажал на кнопку {buttonText}");
            
            try
            {
                if (buttonText == "Назад")
                {
                    try
                    {
                        await Bot.DeleteMessageAsync(e.CallbackQuery.From.Id, e.CallbackQuery.Message.MessageId - 1);
                        await Bot.DeleteMessageAsync(e.CallbackQuery.From.Id, e.CallbackQuery.Message.MessageId);
                    }
                    catch 
                    { 

                    }
                }
                else if (buttonText == "Добавить данные")
                {
                    //await Bot.DeleteMessageAsync(e.CallbackQuery.From.Id, e.CallbackQuery.Message.MessageId + 1);
                    await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "Напишите через запятую ваши хобби (5 штук)");

                    //message_id = e.CallbackQuery.Message.MessageId;
                    //string test = e.CallbackQuery.Message.Text;
                }
                else if (buttonText == "Изменить данные")
                {
                    //try
                    //{
                    //    await Bot.DeleteMessageAsync(e.CallbackQuery.From.Id, e.CallbackQuery.Message.MessageId + 1);
                    //}
                    //catch { }
                    
                    await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "Напишите новые запятую ваши хобби (5 штук)");
                    //message_id = e.CallbackQuery.Message.MessageId;
                }
                else if (buttonText == "Найти людей")
                {
                    await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "Начинаю поиск...");
                    //message_id = e.CallbackQuery.Message.MessageId;

                    string answer_message = search_users(e.CallbackQuery.From.Id);


                    //await Bot.DeleteMessageAsync(e.CallbackQuery.From.Id, e.CallbackQuery.Message.MessageId + 1);
                    await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, answer_message);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            
            //await Bot.SendTextMessageAsync(e.CallbackQuery.Id, $"{buttonText}");
            
        }

        private static async void BotOnMessageReceived(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;
            if (message == null || message.Type != MessageType.Text)
            {
                return;
            }
            string user_name = $"{message.From.FirstName} {message.From.LastName} {message.From.Id} @{message.From.Username}";
            Console.WriteLine($"{user_name} отправил:  '{message.Text}'");
            /*
            if (check_method == true)
            {
                check_dilog(message.From.Id, message.From.FirstName, "@" + message.From.Username);
            }
            */
            
            
            string Users_message = "Выберите, что хотите сделать или нажмите \"помощь\", для прочтение подсказки";


            switch (message.Text)
            {
                
                case "/start":
                    check_method = true;
                    if (check_method == true)
                    {
                        check_dilog(message.From.Id, message.From.FirstName, "@" + message.From.Username);
                        if (users_autorization == true)
                            number_button_info = "Добавить данные";
                        else if (users_autorization == false)
                            number_button_info = "Изменить данные";
                    }

                    var inlinekeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData(number_button_info),
                            InlineKeyboardButton.WithCallbackData("Найти людей"),
                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Помощь")
                        }
                    });
                    await Bot.SendTextMessageAsync(message.Chat.Id, Users_message, replyMarkup: inlinekeyboard);
                    
                    break;

                case "/help":
                    string info_message = "Добрый день! " +
                                        "\n Данный бот может помочь тебе найти человека, " +
                                        "\n с которым тебе было бы интересно общаться. " +
                                        "\n Если у тебя совпадут более, чем 2 хобби, " +
                                        "\n то вы сможете познакомиться!!!" +
                                        "\n\n Скорее нажимай на \"Назад\", затем на \"Найти людей\" и закомься)";
                    var back_button = new InlineKeyboardMarkup(new[]
                    {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Назад")
                        }
                    });
                    await Bot.SendTextMessageAsync(message.Chat.Id, info_message, replyMarkup: back_button);

                    break;

                case "/clear":
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Подождите немного...");
                    for (int i = e.Message.MessageId - 20; i < e.Message.MessageId + 20; i++)
                    {
                        try
                        {
                            await Bot.DeleteMessageAsync(e.Message.Chat.Id, i);
                        }
                        catch
                        { }
                    }

                    if (users_autorization == true)
                        number_button_info = "Добавить данные";
                    else if (users_autorization == false)
                        number_button_info = "Изменить данные";

                    var inlinekeyboard_after_clear = new InlineKeyboardMarkup(new[]
                    {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData(number_button_info),
                            InlineKeyboardButton.WithCallbackData("Найти людей"),
                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Помощь")
                        }
                    });
                    await Bot.SendTextMessageAsync(message.Chat.Id, Users_message, replyMarkup: inlinekeyboard_after_clear);
                    break;

                default:
                    if (Users_answer == "Добавить данные")
                    {
                        string error = data_base(false ,message.Text, message.From.Id);

                        for (int i = message_id - 20; i < message_id + 20; i++)
                        //{
                        //    try
                        //    {
                        //        await Bot.DeleteMessageAsync(e.Message.Chat.Id, i);
                        //    }
                        //    catch
                        //    { }
                        //}

                        if (users_autorization == true)
                            number_button_info = "Добавить данные";
                        else if (users_autorization == false)
                            number_button_info = "Изменить данные";

                        var inlinekeyboard_after_add = new InlineKeyboardMarkup(new[]
                        {
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData(number_button_info),
                                InlineKeyboardButton.WithCallbackData("Найти людей"),
                            },
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("Помощь")
                            }
                        });

                        await Bot.SendTextMessageAsync(message.Chat.Id, Users_message, replyMarkup: inlinekeyboard_after_add);
                        if (error != "")
                            await Bot.SendTextMessageAsync(message.Chat.Id, error);

                    }
                    else if (Users_answer == "Изменить данные")
                    {
                        string error = data_base(true, message.Text, message.From.Id);
                        for (int i = message_id - 2; i < message_id + 10; i++)
                        {
                            try
                            {
                                await Bot.DeleteMessageAsync(e.Message.Chat.Id, i);
                            }
                            catch
                            {
                                
                            }
                        }

                        if (users_autorization == true)
                            number_button_info = "Добавить данные";
                        else if (users_autorization == false)
                            number_button_info = "Изменить данные";

                        var inlinekeyboard_after_upgrade = new InlineKeyboardMarkup(new[]
                        {
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData(number_button_info),
                                InlineKeyboardButton.WithCallbackData("Найти людей"),
                            },
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("Помощь")
                            }
                        });

                        await Bot.SendTextMessageAsync(message.Chat.Id, Users_message, replyMarkup: inlinekeyboard_after_upgrade);
                        if (error != "")
                            await Bot.SendTextMessageAsync(message.Chat.Id, error);
                    }
                    else if (Users_answer == "Найти людей")
                    {
                        await Bot.DeleteMessageAsync(e.Message.Chat.Id, message_id + 1);        //
                        await Bot.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId);   // Удаляет последнее сообщение

                    }
                    else
                    {
                        //for (int i = e.Message.MessageId - 2; i <= e.Message.MessageId + 10; i++)
                        //{
                        //    try
                        //    {
                        //        await Bot.DeleteMessageAsync(e.Message.Chat.Id, i);
                        //    }
                        //    catch
                        //    {
                        //        if (i != e.Message.MessageId - 2)
                        //            break;
                        //    }
                        //
                        //await Bot.SendTextMessageAsync(message.Chat.Id, Users_message, replyMarkup: inlinekeyboard);
                        //await Bot.SendTextMessageAsync(message.From.Id, "Я вас не понимаю!");

                    }
                    Users_answer = "";
                    break;
            }
        }

    }
}

using Microsoft.Data.Sqlite;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
var botClient = new TelegramBotClient("5379398509:AAG-bJQ4MnbjuTxLP-wiFLbcW97lz3mRxCs");
using var cts = new CancellationTokenSource();
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = { } // receive all update types
};
int jokenumber = 0; //номер шутки которая будет озвучиваться
int numberofemptyslots = 0;//количество свободных слотов (пока что бесполезно)
int numberofjokes = 0; ; //колво шуточек в базе данных
const int x = 10;
long[] manualjokenumber = new long[x];//для функции включения введения номера шутки
long[] writingjoke = new long[x];   //для функции написания шутки
long[] ispublic = new long[x]; // для проверки готовности побликации шутки
long[] isnameenter = new long[x]; // ввод имени пользователя
//bool haswrittenjoke = false; //для доступа к базе данных и управление записью(бесполезно)
string[] publicjoke = new string[x];  //строка для записи шутки (подвязана под ispublic)
long chatId;
string messageText;
Random rand = new Random();
ReplyKeyboardMarkup replyKeyboardMarkup1 = new(new[]
    {
        new KeyboardButton[] { "Random joke", "joke by number", "Upload your own joke" },
    })
{
    ResizeKeyboard = true
};
ReplyKeyboardMarkup replyKeyboardMarkup2 = new(new[]
{
        new KeyboardButton[] { "Повернутися назад" },
    })
{
    ResizeKeyboard = true
};
ReplyKeyboardMarkup replyKeyboardMarkup3 = new(new[]
{
        new KeyboardButton[] { "Так" , "No"},
    })
{
    ResizeKeyboard = true
};

string cnnstring = "Filename = database888.db; mode = ReadWriteCreate";
var cnn = new SqliteConnection(cnnstring);
try
{
    cnn.Open();
    Console.WriteLine("glhf");
    cnn.Close();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

botClient.StartReceiving(
    HandleUpdateAsync,
    HandleErrorAsync,
    receiverOptions,
    cancellationToken: cts.Token);
var me = await botClient.GetMeAsync();
//Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");
Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Type != UpdateType.Message)
    {
        return;
    }
    Console.WriteLine($"User {update.Message.From} has written ({update.Message.Text})");
    chatId = update.Message.Chat.Id;
    messageText = update.Message.Text;


    for(int i = 0; i < 10; i++)
    {
        if (manualjokenumber[i] == chatId)
        {
            try
            {

                jokenumber = Convert.ToInt32(messageText);
                if(jokenumber>numberofjokes  || jokenumber <= 0)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Ты не попал в диапазон xd",
                        cancellationToken: cancellationToken);
                    return;
                }
                else
                {
                    manualjokenumber[i] = 0;
                    dbreader(jokenumber, cancellationToken);
                    return;

                }
            }
            catch(Exception)
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "цифру введи довбик",
                    cancellationToken: cancellationToken);
                return;
            }
        }
        if(writingjoke[i] == chatId)
        {
            switch (messageText.ToLower())
            {
                case "повернутися назад":
                    writingjoke[i] = 0;
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text:"okok",
                        replyMarkup:replyKeyboardMarkup1,
                        cancellationToken: cancellationToken);
                    return;
                default:
                    writingjoke[i] = 0;
                    boolcheckerwriter(3);
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "вы точно хотите опубликовать joke?",
                        replyMarkup:replyKeyboardMarkup3,
                        cancellationToken: cancellationToken);
                    return; 
            }
        }
        if(isnameenter[i] == chatId)
        {
            dbwriter(messageText,publicjoke[i], cancellationToken);
            isnameenter[i] = 0;
            ispublic[i] = 0;
            publicjoke[i] = "";
            return;
        }
        else if(ispublic[i] == chatId)
        {
            switch (messageText.ToLower())
            {
                case "так":
                    boolcheckerwriter(4);
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Write your name",
                        cancellationToken: cancellationToken);
                    return;
                case "no":
                    ispublic[i] = 0;
                    publicjoke[i] = "";
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "okok",
                        replyMarkup: replyKeyboardMarkup1,
                        cancellationToken: cancellationToken);
                    return;
            }
        }
    }
    //int[] emptyslots = new int[numberofjokes];         !!!!!!!!!!!!!!!!! важно

    switch (messageText.ToLower())
    {
        case "/start":
            start(cancellationToken);
            return;
        case "random joke":
            numberofjokes = dbreaderamount();
            jokenumber = rand.Next(1, numberofjokes + 1);
            dbreader(jokenumber, cancellationToken);
            return;
        case "joke by number":
            numberofjokes = dbreaderamount();
            await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"wee have {numberofjokes} jokes, choose one",
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken
                    );
            boolcheckerwriter(1);
            return;
        case "upload your own joke":
            boolcheckerwriter(2);
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "So, write your own joke",
                replyMarkup: replyKeyboardMarkup2,
                cancellationToken: cancellationToken
                );
            return;
        default:
            break;

    }















}
Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };
    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}



async void start(CancellationToken cancellationToken)
{
    Message startmessage = await botClient.SendTextMessageAsync(
       chatId: chatId,
       text: "wellcome to jojo joke bot (here i send jokes collected by jojo) ",
       replyMarkup: replyKeyboardMarkup1,
    cancellationToken: cancellationToken);
    return;
}
int dbreaderamount()
{
    int numberofjokes = 0;
    string cmdtext = "SELECT * FROM Table1";
    SqliteCommand commandread = new SqliteCommand(cmdtext, cnn);
    cnn.Open();
    SqliteDataReader reader = commandread.ExecuteReader();
    while (reader.Read())
    {

        if (DBNull.Value.Equals(reader.GetValue(2)) == true)
        {
        }
        else
        {
            numberofjokes++;
        }
    }
    return numberofjokes;
}
async void dbreader(int jokenumber, CancellationToken cancellationToken)
{
    int number = 1;
    string cmdtext = "SELECT * FROM Table1";
    SqliteCommand commandread = new SqliteCommand(cmdtext, cnn);
    cnn.Open();
    SqliteDataReader reader = commandread.ExecuteReader();
    while (reader.Read())
    {

        if (DBNull.Value.Equals(reader.GetValue(2)) == false)
        {
            if(number == jokenumber)
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"joke by {reader.GetValue(1)} : \n {reader.GetString(2)}",
                     replyMarkup: replyKeyboardMarkup1,
                    cancellationToken: cancellationToken
                   
                    );
                return;
            }
            else
            {
                number++;
            }
        }

    }
}
async void dbwriter(string namename, string publicjoke, CancellationToken cancellationToken)
{
    string cmdtext =$"INSERT INTO Table1 (username, joke) VALUES ('{namename}','{publicjoke}')";
    SqliteCommand command1 = new SqliteCommand(cmdtext, cnn);
    cnn.Open();
    command1.ExecuteNonQuery();
    cnn.Close();
    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Ваш joke сохранен успешно",
                    replyMarkup: replyKeyboardMarkup1,
                    cancellationToken: cancellationToken
                    );
}
int boolcheckerwriter(int casing)
{
    switch (casing)
    {
        case 1:
            for(int i = 0; i < 10; i++)
            {
                if(manualjokenumber[i] == 0)
                {
                    manualjokenumber[i] = chatId;
                    return 0;
                }
            }
            break;
        case 2:
            for (int i = 0; i < 10; i++)
            {
                if (writingjoke[i] == 0)
                {
                    writingjoke[i] = chatId;
                    return 0;
                }
            }
            break;
        case 3:
            for (int i = 0; i < 10; i++)
            {
                if (ispublic[i] == 0)
                {
                    ispublic[i] = chatId;
                    publicjoke[i] = messageText;
                    return 0;
                }
            }
            break;
        case 4:
            for (int i = 0; i < 10; i++)
            {
                if (isnameenter[i] == 0)
                {
                    isnameenter[i] = chatId;
                    return 0;
                }
            }
            break;
        default:
            break;
    }
    return 0;
}
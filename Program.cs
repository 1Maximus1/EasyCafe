using System.Runtime.InteropServices;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseStaticFiles();

app.Use(async(context, next) =>
{
    var path = context.Request.Path;
    var fullpath = $"wwwroot/index/htmlpage.html";
    var response = context.Response;
    response.ContentType = "text/html; charset=utf-8";
    if (File.Exists(fullpath))
    {
        await response.SendFileAsync(fullpath);
    }
    await next.Invoke();
});



app.Use(async (context, next) =>
{
    
    if (context.Request.Path == "/user_menu")
    {
        var response = context.Response;
        StringBuilder answer = new StringBuilder();
        Dictionary<string, int> menu = new Dictionary<string, int>(){
            { "None", 0 },
            { "Yogurt with granola and fruit", 60 },
            { "Chia pudding with coconut milk and berries", 70 },
            { "Smoothie bowls with fruits and nuts", 85 },
            { "Cheese plate with nuts and honey", 90 },

            { "Omelette with cheese and vegetables", 100 },
            { "Oatmeal with fruit and honey", 65 },
            { "Eggs Benedict with salmon", 120 },
            { "Pancakes with berries and sour cream", 80 },

            { "Borsch with sour cream", 95 },
            { "Chicken noodle soup", 85 },
            { "Mushroom soup", 90 },
            { "Solyanka", 100 },

            { "Chicken Kyiv with mashed potatoes", 150 },
            { "Chicken chop with vegetable garnish", 140 },
            { "Fried salmon with rice", 200 },
            { "Pasta carbonara", 130 },

            { "Coffee", 30 },
            { "Tea", 20 },
            { "Hot chocolate", 40 },
            { "Cola", 25 },
            { "Sprite", 25 },
            { "Cold tea", 20 },
            { "Mineral water", 15 },
            { "Orange juice", 35 },
            { "Apple juice", 35 },
            { "Tomato juice", 30 }
        };

        int sumOfOrder = 0;
        Dictionary<string, int> userOrder = new Dictionary<string, int>();
        int currentHour = DateTime.Now.Hour;

        var form = context.Request.Form;

        string breakfastStr = form["user_breakfast"];
        string firstCourseStr = form["user_first_course"];
        string secondCourseStr = form["user_second_course"];
        string drinkStr = form["user_drink"];

        answer.Append("<div class=\"answer-container\">");
        int breakfastPrice = 0;
        if (currentHour >= 7 && currentHour < 11)
        {
            breakfastPrice = menu[breakfastStr];
        }
        else if (breakfastStr != "None")
        {
            answer.Append("<p class=\"answer\">Breakfast is only available from 7 to 11 a.m. Your breakfast selection was not included.</p>");
        }

        int firstCoursePrice = menu[firstCourseStr];
        int secondCoursePrice = menu[secondCourseStr];
        int drinkPrice = menu[drinkStr];

        if (breakfastStr == "None" && firstCourseStr == "None" && secondCourseStr == "None" && drinkStr == "None")
        {
            answer.Append("<p class=\"answer\">You didn't order anything</p></div>");
            await response.WriteAsync(answer.ToString());
        }
        else
        {
            if (breakfastStr != "None" && breakfastPrice != 0 && !userOrder.ContainsKey(breakfastStr))
                userOrder.Add(breakfastStr, breakfastPrice);

            if (firstCourseStr != "None" && !userOrder.ContainsKey(firstCourseStr))
                userOrder.Add(firstCourseStr, firstCoursePrice);

            if (secondCourseStr != "None" && !userOrder.ContainsKey(secondCourseStr))
                userOrder.Add(secondCourseStr, secondCoursePrice);

            if (drinkStr != "None" && !userOrder.ContainsKey(drinkStr))
                userOrder.Add(drinkStr, drinkPrice);

            sumOfOrder = userOrder.Values.Sum();
            answer.Append("<p class=\"answer\">Positions:</p>");
   
            foreach (var entry in userOrder)
            {
                answer.Append($"<p class=\"answer\">{entry.Key}: {entry.Value} грн</p>");
                
            }
            answer.Append($"<p class=\"answer\">Total: {sumOfOrder} грн</p>");
            answer.Append($"<p class=\"answer\">Date: {DateTime.Now}</p></div>");

            
            await context.Response.WriteAsync(answer.ToString());
        }
    }
    await next.Invoke();
});

app.Run(async (context) =>
{
    var request = context.Request;

    Console.WriteLine($"Method: {request.Method}");

    Console.WriteLine($"Path: {request.Path}");

    Console.WriteLine($"Path with QueryString: {request.Path + request.QueryString}");

    Console.WriteLine("Headers:");
    foreach (var header in request.Headers)
    {
        Console.WriteLine($"  {header.Key}: {header.Value}");
    }

    Console.WriteLine("Query Parameters:");
    foreach (var queryParam in request.Query)
    {
        Console.WriteLine($"  {queryParam.Key}: {queryParam.Value}");
    }

    Console.WriteLine($"Client IP: {context.Connection.RemoteIpAddress}");

    Console.WriteLine($"Host: {request.Host}");

    Console.WriteLine($"Scheme: {request.Scheme}");

    if (request.Method == "POST" || request.Method == "PUT")
    {
        request.EnableBuffering();
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        Console.WriteLine("Body:");
        Console.WriteLine(body);
    }

    if (request.HasFormContentType && request.Form.Files.Count > 0)
    {
        Console.WriteLine("Files:");
        foreach (var file in request.Form.Files)
        {
            Console.WriteLine($"  {file.Name} - {file.FileName} ({file.Length} bytes)");
        }
    }

    if (request.HasFormContentType)
    {
        Console.WriteLine("Form Data:");
        foreach (var formField in request.Form)
        {
            Console.WriteLine($"  {formField.Key}: {formField.Value}");
        }
    }
    Console.WriteLine("==============================================================");
});
app.Run();
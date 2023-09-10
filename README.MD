# PersonalProjects

## LOLApi:
I would really like to hand over this project to Nymphx2, nerd moment of the day, tired of being camped by people who play DUO with the Jungle. Solution? Create an app that checks who's DUO, using a heuristic from the match history of everyone in the game, utility over nine thousand.

## TelegramBot:
That one is like a diary, a simple CRUD(Create, Read, Update, Delete) using LorealPersistor through a Telegram bot API, anywhere you're if you have access to Telegram you have all your things, never ever forget something again.

## Miracs2:
This project serves several purposes. The first one arose when I was playing CS with a scout and needed a centralized crosshair. So, the first characteristic of this system is to place a dot in the center of the screen. It evolved with 'what if I also have a dot to know where the spray of the shot will go?' That's when I started putting a second dot on the screen, HUE HUE HUE... One day, I was playing Hell Let Loose, and I couldn't see the enemies, but they could see me. So the idea came up, 'what if I create a super ZOOM that works with any weapon?' HUE HUE HUE². So the third function of this project is to create a ZOOM from the center of the screen on a second monitor, running at an incredible 75fps. Yes, it resizes the center of the screen by 2x and replicates the images at 75FPS on a second monitor (without using Cuda Cores), 0 garbage collector, fixed memory allocation. I'm quite proud of this project; now I can complain about the Windows magnifier. 

## Many Utils classes, among them:
### {
  ### FTP connection Handler:
  Aid in checking if a file exists, copying, pasting, adding, and so on.
  
  ### ImageHandler:
  The idea of the FTP was to upload images, but as every good programmer knows, we always have to doubt the imaginative capacity of our users. That's when this class was conceived: to compress images to 1200x1200 and slightly change their characteristics without a very noticeable loss of quality, in order to prevent them from uploading images in 8k super ultra resolution HDR, which would fill the FTP in 5 seconds and then they would start complaining that it takes too long to download the image back.
  
  ### Bool:
  Convert INT to Bool, 0 e 1, 1 e 0.
  
  ### Expressions:
  Auxiliary methods of LorealPersistor (my 'EF'), converting LINQ to ADO SQL, a powerful class that is constantly under development. In short, EF wasn't built overnight, let alone by a single Renan.
  
  ### ArrayInsert:
  Based on an object that is an array of Double, Int, String, it returns an array of object (honestly Renan, you can do better than this messy code).
  
  ### ClassToStringSeparated:
  Converts a class into csv(Comma separated file).
  
  ### ReadFile:
  It reads a file, I hope no one ever sends a large file, because honestly... I expected more (unfortunately, this way of reading the file made sense to the past Renan Jr., fix it as soon as possible).
  
  ### StringExtension:
  String extensions, for example ReplaceEmpty, because it's a pain to keep using string.replace(target, "") uma seguida da outra, tipo string.replace(target, "").replace(target2, "").replace(target3, ""), IsAnyEmpty, checks in a string params if any position is empty, TakeRight and TakeLeft something close to excel LEFT e RIGHT functions.

  ### TypeHelper:
  Some Type helpers, converting C# types to SQL, discovering the type of a generic property, converting an object to DATETIME, among others
### }

## SQLServer

In the future, my masterpiece. I never really liked Entity Framework; I find its implementation somewhat rigid. But anyone can complain, right? That's when LorealPersistor was born. The idea here is a replacement for EF, with easily implemented classes, with 'wheres' entirely done in LINQ and converted to ADO (which theoretically always wins in terms of performance over EF). Select, Insert, InsertOrUpdate, Update, and in the future, joins and even easier-to-implement classes. After all, why do we specify the primary key and foreign keys if they are already in the database? Why not just create a class and say it belongs to table X, and start selecting and inserting as if there's no tomorrow?

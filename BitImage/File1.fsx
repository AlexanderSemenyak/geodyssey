#light

#light

// This file is a script that can be executed with the F# Interactive.  
// It can be used to explore and test the library project.
// Note that script files will not be part of the project build.

#I @"C:\Users\rjs\Documents\dev\p4workspace\sandbox\geodyssey\proto\BitImage\bin\Debug"
#I @"C:\Users\rjs\Documents\dev\p4workspace\sandbox\geodyssey\proto\ClassLibrary1\bin\Debug"
#I @"C:\Users\rjs\Documents\dev\p4workspace\sandbox\geodyssey\proto\Image\bin\Debug"
#r @"Image.dll"

#r "System.Core.dll"

#load "Utility.fs"

// -- Testing --
  
let testData1= [ "0000000000000000" ;
                 "0000000000000000" ;
                 "0001110000000000" ;
                 "0011111000000000" ;
                 "0011111000011100" ;
                 "0011110000111100" ;
                 "0001100001111100" ;
                 "0000000011111000" ;
                 "0010000111110000" ;
                 "0000001111100000" ;
                 "0000011111000000" ;
                 "0000111110000000" ;
                 "0000111111110000" ;
                 "0000111111110000" ;
                 "0000011111100000" ;
                 "0000000000000000" ]

let testData2= [ "0000000000000000" ;
                 "0000000000000000" ;
                 "0110000000000000" ;
                 "0001000100000000" ;
                 "0111111111000000" ;
                 "0111111111110000" ;
                 "0111111111111000" ;
                 "0111111111111100" ;
                 "0111000001111100" ;
                 "0110000000111100" ;
                 "0110000000011100" ;
                 "0100000000001100" ;
                 "0100000000001100" ;
                 "0100000000000100" ;
                 "0011100000000000" ;
                 "0000000000000000" ]

let testData3= [ "0000000000000000" ;
                 "0000000000000000" ;
                 "0110000000000000" ;
                 "0001000100000000" ;
                 "0111111111000000" ;
                 "0111111111110000" ;
                 "0111111111111000" ;
                 "0111111111111100" ;
                 "0111000001111100" ;
                 "0110000000111100" ;
                 "0110000000011100" ;
                 "0100000000001100" ;
                 "0100000000001100" ;
                 "0100000000000100" ;
                 "0011100000000000" ;
                 "0000000000000000" ]
  
let testData4= [ "0000010000000000" ;
                 "0000010000000000" ;
                 "0000100000000000" ;
                 "0000100000000000" ;
                 "0000100000000000" ;
                 "0000111000000000" ;
                 "0000110000000000" ;
                 "0011110000000000" ;
                 "0100010000000000" ;
                 "1000001000000000" ;
                 "1000000100000000" ;
                 "0000000100000000" ;
                 "0000000100000000" ;
                 "0000000010000000" ;
                 "0000000010000000" ;
                 "0000000001000000" ]

let testData5= [ "0000000000010000" ;
                 "0001000000010000" ;
                 "0000100001111100" ;
                 "0000010000010000" ;
                 "0000001000010000" ;
                 "0000000111110000" ;
                 "0000000000010000" ;
                 "0000000000010000" ;
                 "0001110000010000" ;
                 "0000011100010000" ;
                 "0000000010100000" ;
                 "0000000001000000" ;
                 "0000000010100000" ;
                 "0000000100010000" ;
                 "0000000000000000" ;
                 "0000000000000000" ]
                 
// -- Test Execution --

let d1 = testData1 |> BitImage.Utility.stringListToBools |> List.rev |> BitImage.Utility.array2FromListOfLists;;

let testImage1 = new Image.FastImage<bool>(d1);;
//let testImage2 = new BinaryImage(testData2 |> stringListToBools |> List.rev |> array2FromListOfLists);;
//let testImage3 = new BinaryImage(testData3 |> stringListToBools |> List.rev |> array2FromListOfLists);;
//let testImage4 = new BinaryImage(testData4 |> stringListToBools |> List.rev |> array2FromListOfLists);;
//let testImage5 = new BinaryImage(testData5 |> stringListToBools |> List.rev |> array2FromListOfLists);;





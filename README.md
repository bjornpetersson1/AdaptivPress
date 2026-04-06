 # AdaptivPress                                               
  
  AdaptivPress is a proof-of-concept .NET console application that compresses sequences of data by finding the mathematical rules behind them.                             

  Feed it a list of numbers, or a piece of text, and instead of storing every value it works out what generated them: an arithmetic progression, a geometric series, a power function, a repeating block, or some combination of the above. Characters in a string are treated as their numeric codes, compressed the same way as any other sequence, and decoded back to readable text on output.

  It will not compress everything. But when your data has structure, it will find it.

### Video of using AdaptivPress  

https://github.com/user-attachments/assets/648db886-e0e2-432d-a3d4-04b24cb78cdf



https://github.com/user-attachments/assets/f2e6fe01-4578-4e81-8a8f-2ded0e5b553b

  ## How it works

  AdaptivPress scans the input three values at a time and asks:
   is there a rule that takes each value to the next?  
  If the same rule holds for three or more values in a row, that run gets stored as a single compressed item (a starting value and a rule) rather than a list of numbers.  

  On top of rule-based compression, AdaptivPress also detects repeating blocks (subsequences that appear two or more times in a row.)

  Anything left over that doesn't fit any pattern is stored as-is. The full result is decompressed and verified against the original before being displayed.

  ## Usage

  ```
  dotnet run
  ```

  Enter values at the prompt, one at a time. The first entry determines the input mode: a valid number puts you in numeric mode, anything else (a word, a sentence, a string of characters) puts you in text mode. Type `E` when done.  

  ## Requirements

  .NET 10.0. No external dependencies.


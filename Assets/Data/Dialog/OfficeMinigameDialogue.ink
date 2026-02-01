#minigame: trainjourney
VAR startMinigame = false
VAR dialogText = ""
->OfficeDialog


=== OfficeDialog ===
{dialogText}
* [OK]
        I'm sure you'll do great.
        ->MINIGAME1


* [Maybe Later]
    OK, Let me know if you have some time later.
    * * [OK]
-> END

=== MINIGAME1 ===
Click button to start minigame.
* [Start Game]
~startMinigame = true
startMinigame = {startMinigame}
        -> DONE
    

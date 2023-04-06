```
Author:     Mason Sansom
Partner:   Dhruv Rachakonda
Date:       3-March-2023
Course:     CS 3500, University of Utah, School of Computing
GitHub ID:  loporlp
Repo:       https://github.com/uofu-cs3500-spring23/assignment-seven---chatting-ms_office_dev_team
Date:       18-Feb-2023
Solution:   Spreadsheet
Copyright:  CS 3500 and Mason Sansom / Dhruv Rachakonda - This work may not be copied for use in Academic Coursework.
```

# Overview of the Spreadsheet functionality

This Project represents a chatting system with a server and a client where multiple clients can
connect to one server and when they send a message to the server it will be sent to all clients connected.

We decided to use a ListView for the clients chat box and participants list because it made it so all we had
to do was update a list and the item added would be put on the GUI in a nice animation. We elected not to do
this for the server because it was a little less work and looks weren't as big of a priority

Most of this project was done using pair programming but the Networking branch was made to go work on the 
Networking object while the main branch could still use the .dll file for testing purposes. The Networking branch was
never officially merged because of a missing .gitignore but the code was moved over successfully
when fixing the issue.

We tested this project by writing down all the possible edge cases we think could be contained in it 
then trying to make them happen. We also placed many breakpoints and tried to trigger them so we would
be sure that the code was running everywhere. Overall we did a lot of manual brute testing. When we tested
the server we used the given client.exe and vice versa so we were sure we had a working model to test our 
additions on


# Time Expenditures:

23.5 Hours
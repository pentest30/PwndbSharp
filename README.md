# PwndbSharp
Search for credentials leaked on pwndb by domain and usernames. it generates two files one for passwords and one for emails.

# Use :
 PwndbParser.exe pwndb yourDomaine [options]
 
 # Arguments:
 
  Domain name

Options:

  -u | --userName    <TEXT>
  Username

  -t | --timeOut     <NUMBER>
  Time out -default 60 000 milliseconds

  -p | -- topProt    <NUMBER>
  Tor port - default port : 9150

  -o | --outputFile  <TEXT>
  Output file
 

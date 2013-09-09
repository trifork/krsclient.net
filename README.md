#krsclient.net  
##Kopi register service - .NET demo client  
  
Klienten viser hvordan kopi register servicen kan kaldes fra .NET  
  
Følgende skal være opfyldt for den virker:  

1: Der ligge et .p12 voces certifikat i Resources mappen.  
2: Workdir skal være roden af projektet.  
3: Whitelisting: for at kalde kopi register servicen skal CVR være whitelistet på NSPen, se mere på www.nspop.dk
  
Jeg kan anbefale at kigge app.config igennem for indstillinger.  

Da dette blot er en demo gemmer klienten blot i en lokal sql ce database, det kan anbefales at som minimum udskifte persisterings delen.  
  
Obs. demoen henter som udgangspunkt 2 ddv register og de indeholder ret meget data så første kørsel kan tage lang tid.

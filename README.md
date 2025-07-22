# SentenceRecovery

1. Launch:

    Run bin\Debug\net8.0\SentenceRecovery.exe
  
    Or bin\Release\net8.0\SentenceRecovery.exe

    Or open as a project using SentenceRecovery.sln.

    The input file "source.txt" should be located next to "SentenceRecovery.exe".
  
    The output file is "result.txt".
   

  
3. Description:
   
    This tool implements an approach for automatic recovery of concatenated or corrupted text using

      splitting the input string into overlapping chunks
   
      recursive processing of each chunk using a dictionary;
   
      combining results based on quality (number of words + weighted score).
   

    Candidate quality is evaluated using word and phrase frequency data from external corpora:
  
      SUBTLEXus (subtitle word frequencies):
   
        https://www.ugent.be/pp/experimentele-psychologie/en/research/documents/subtlexus
    
      Google Books Ngram (massive frequency data from books):
   
        https://github.com/orgtre/google-books-ngram-frequency/tree/main/ngrams


4. Note:
   
   The following initial string for recovery is not valid, as it violates the declared task rules (insertions are indicated with arrows)
   
     Rules:
   
      Some letters are replaced with the * symbol (1 asterisk = 1 letter);
   
      In some words, letters are randomly shuffled;
   
      All spaces and punctuation marks have been removed.
   
```
   Alice was beginning to get very  tired of sitting by her sister on the bank and of having nothing to do once or twice  she had peeped
                                 |                                     |                                             |      |   |
                                 v                                     v                                             v      v   v  
   Al*ce w*s begninnig to egt ver*y tried of sitt*ng by h*r sitsre on ht* bnak and of h*ving nothi*g to do onc* or tw*ice sh*e hd pee*ed


   into the book her sister was reading but it  had no pictures or conversations  in it and what is the use of a book thought Alice without pictures or conversations
               |  |                          |                      |
               v  v                          v                      v
   into th* boo*k hr siste* was r*adnig but i*t had no pictu*es or c*onve*sati*ns in it and what is th* use of a b**k th*ught Alic* withou* pic*u*es or co*versa*ions
```

   That’s why I used the corrected string:
```
Al*cew*sbegninnigtoegtver*triedofsitt*ngbyh*rsitsreonhtebnakandofh*vingnothi*gtodoonc*ortw*cesh*hdapee*edintoth*boo*h*rsiste*wasr*adnigbuti*hadnopictu*esorc*nve*sati*nsinitandwhatisth*useofab**kth*ughtAlic*withou*pic*u*esorco*versa*ions
```
   

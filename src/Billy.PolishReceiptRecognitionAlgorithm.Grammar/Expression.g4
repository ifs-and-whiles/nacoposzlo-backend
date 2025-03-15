grammar Expression;

productsexp
 : products EOF
 ;

letter
 : A | PLA	| B | C	| PLC | D | E | PLE | F	| G | H	| I | J	| K | L	| PLL | M | N | PLN | O	| PLO | P | Q | R | S | PLS | T	| U | V	| W | X	| Y | Z	| PLZ | PLZZ
 ;

word
 : (letter | OTHER)+
 ;

unit
 : word
 ;

quantity
 : qt=DECIMAL SP? (MUL | X)
 | qt=DECIMAL SP? unit
 | qt=DECIMAL SP? unit SP? (MUL | X)
 | qt=DECIMAL
 ;

currency
 : Z L OTHER? 
 | Z PLL OTHER? 
 ;

unitPrice
 : up=DECIMAL (SP? currency)?
 ;

amount
 : am=DECIMAL (SP? taxTag=word)?
 ;

name
 : (DECIMAL | letter | OTHER | SP | MUL | EQ)+?
 ;
 
unrecognizedSequence
 : (DECIMAL | letter | OTHER | SP | MUL | EQ)+
 ;

singleLineProduct
 : name SP quantity SP? unitPrice SP? EQ? SP? amount SP?
 ;

multiLinesProduct
 : name NL quantity SP? unitPrice SP? EQ? SP? amount SP?
 | name SP quantity SP? unitPrice NL amount SP?
 ;

singleLineMissingQuantityProduct
 : name SP unitPrice SP? EQ? SP? amount SP?
 ;

multiLinesMissingQuantityProduct
 : fln=name NL sln=name unitPrice SP? EQ? SP? amount SP?
 ;

singleLineMissingQuantityAndUnitPriceProduct
 : name SP? EQ? SP? amount SP?
 ;

products
 : singleLineProduct
 | multiLinesProduct
 | singleLineMissingQuantityProduct
 | singleLineMissingQuantityAndUnitPriceProduct	
 | unrecognizedSequence NL singleLineProduct
 | multiLinesMissingQuantityProduct
 | unrecognizedSequence		 			
 | products NL products									
 ;
  
DECIMAL	: ('-'[ ]*)?[0-9]+ | ('-'[ ]*)?[0-9]+ ([ ]*('.' | ',')[ ]*) [0-9]+ ;

A	: [aA] ;
PLA	: '\u0105' | '\u0104' ;
B	: [bB] ;
C	: [cC] ;
PLC : '\u0107' | '\u0106' ;
D	: [dD] ;
E	: [eE] ;
PLE	: '\u0119' | '\u0118' ;
F	: [fF] ;
G	: [gG] ;
H	: [hH] ;
I	: [iI] ;
J	: [jJ] ;
K	: [kK] ;
L	: [lL] ;
PLL	: '\u0142' | '\u0141' ;
M	: [mM] ;
N	: [nN] ;
PLN	: '\u0144' | '\u0143' ;
O	: [oO] ;
PLO : '\u00f3' | '\u00d3' ;
P	: [pP] ;
Q	: [qQ] ;
R	: [rR] ;
S	: [sS] ;
PLS	: '\u015b' | '\u015a' ;
T	: [tT] ;
U	: [uU] ;
V	: [vV] ;
W	: [wW] ;
X	: [xX] ;
Y	: [yY] ;
Z	: [zZ] ;
PLZ	: '\u017a' | '\u0179' ;
PLZZ: '\u017c' | '\u017b' ;

EQ		: '=' | ':' ;
MUL     : '*' | '#' | '+' ;

OTHER	: '-' | '_' | '{' | '}' | '[' | ']' | '/' | '\\' | '%' | '.' | ',' | '?' ;
SP		: [ ]+ ;
NL		: [\r][\n] ;
#trivial word with palindroms
test1.txt {ananas}
odd: a,n,a,n,a,s,ana,nan,ana,anana (10)
even: - (0)
answer1.txt - 10 0 10

#trivial word without palindroms
test2.txt {rose}
odd: r,o,s,e (4)
even: - (0)
answer2.txt - 4 0 4

#checking the register
test3.txt {IllIkEeKiLLi}
odd: I,l,l,I,k,E,e,K,i,L,L,i (12)
even: ll,LL,IllI,iLLi (4)
answer3.txt - 16 4 12

#empty word
test4.txt {}
odd: - (0)
even: - (0)
answer4.txt - 0 0 0

#one letter ten times or student's thoughts
test5.txt {aaaaaaaaaa}
odd: a*10 + aaa*8 + aaaaa*6 + aaaaaaa*4 + aaaaaaaaa*2 (30)
even: aa*9 + aaaa*7 + aaaaaa*5 + aaaaaaaa*3 + aaaaaaaaaa*1 (25)
answer5.txt - 55 25 30

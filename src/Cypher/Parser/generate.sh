# Install Java (version 1.6 or higher)
#$ cd /usr/local/lib
#$ curl -O http://www.antlr.org/download/antlr-4.7.1-complete.jar
#Or just download in browser from website: http://www.antlr.org/download.html and put it somewhere rational like /usr/local/lib.

# Add antlr-4.7.1-complete.jar to your CLASSPATH:
export CLASSPATH=".:/usr/local/lib/antlr-4.7.1-complete.jar:$CLASSPATH"

# It's also a good idea to put this in your .bash_profile or whatever your startup script is.

# Create aliases for the ANTLR Tool, and TestRig.
alias antlr4='java -Xmx500M -cp "/usr/local/lib/antlr-4.7.1-complete.jar:$CLASSPATH" org.antlr.v4.Tool'
alias grun='java org.antlr.v4.gui.TestRig'

antlr4 Cypher.g4 -Dlanguage=CSharp -visitor -no-listener
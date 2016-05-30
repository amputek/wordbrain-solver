using System;
using System.Collections.Generic;

public class Board{

    public int width;
    public int height;
    public int numberOfSquares;
    public string[,] board;
    public List<int>[] connections;

    public Board( string[,] aboard ){
        board = aboard;
        width = board.GetLength(0);
        height = board.GetLength(1);
        numberOfSquares = width * height;
    }

    public void Set( Board parent ){
        for( int y = 0; y < height; y++ ){
            for( int x = 0; x < width; x++ ){
                board[x,y] = parent.board[x,y];
            }
        }


    }

    public void SetupConnections(){
        connections = new List<int>[ width * height ];

        for( int y = 0; y < numberOfSquares; y++ ){ connections[y] = new List<int>(); }

        for( int y = 0; y < height; y++ ){
            for( int x = 0; x < width; x++ ){
                int thisIndex = (y * width) + x;
                addEdge( thisIndex, ((y-1) * width) + x ); // above
                if( x != 0 ) addEdge( thisIndex, ((y-1) * width) + (x-1) ); // above left
                if( x != 0 ) addEdge( thisIndex, ((y+0) * width) + (x-1) ); // left
                if( x != 0 ) addEdge( thisIndex, ((y+1) * width) + (x-1) ); // below left
                addEdge( thisIndex, ((y+1) * width) + x ); // below
                if( x != width-1 ) addEdge( thisIndex, ((y+1) * width) + (x+1) ); // below right
                if( x != width-1 ) addEdge( thisIndex, ((y+0) * width) + (x+1) ); // right
                if( x != width-1 ) addEdge( thisIndex, ((y-1) * width) + (x+1) ); // above right
            }
        }
    }

    private void addEdge( int node, int neighbour ){
        if( neighbour >= 0 && neighbour < numberOfSquares ){
            bool alreadyAdded = false;
            for (int n = 0; n < connections[node].Count; n++) {
                if(connections[node][n] == neighbour) alreadyAdded = true;
            }
            if( !alreadyAdded && getLetter(node) != "0" && getLetter(neighbour) != "0" ){
                connections[node].Add(neighbour);
                connections[neighbour].Add(node);
            }
        }
    }

    public bool isClear(){
        for( int y = 0; y < height; y++ ){
            for( int x = 0; x < width; x++ ){
                if(board[x,y] != "0") return false;
            }
        }
        return true;
    }

    //Allows letters to 'drop' down into empty spaces after a word is entered
    public void Update(){
        for( int n = 0; n < 3; n++ ){
            for( int y = 0; y < height-1; y++ ){
                for( int x = 0; x < width; x++ ){
                    if( board[y+1,x] == "0" ){
                        board[y+1,x] = board[y,x];
                        board[y,x] = "0";
                    }
                }
            }
        }
    }

    public void clearLetter( int index ){
        int y = index % width;
        int x = (int)Math.Floor( (double)index / width);
        board[x,y] = "0";
    }

    public string getLetter( int index ){
        int y = index % width;
        int x = (int)Math.Floor( (double)index / width);
        return board[x,y];
    }

    // Converts a route (list of numbers separated by dashes) into the word it represents
    public string ConvertRouteToWord( string route ){
        string[] routes = route.Split('-');
        string word = "";
        for( int i = 0; i < routes.Length; i++ ){
            string p = routes[i];
            if( p != "" ) word = word + getLetter( Int32.Parse(p) );
        }
        return word;
    }

    private string PrintLetter( string s ){
        return (s == "0" ? " " : s);
    }

    public void EnterWord( string route ){
        // Console.WriteLine("Entering: " + ConvertRouteToWord( route ) );
        string[] routes = route.Split('-');
        for (int n = 0; n < routes.Length; n++) {
            string p = routes[n];
            if( p != "" ) clearLetter(Int32.Parse(p));
        }

        Update();
    }

    public void Print(){
        Console.Write(".");
        for( int x = 0; x < width; x++ ){ Console.Write("---."); }
        Console.WriteLine("");
        for( int y = 0; y < height; y++ ){
            Console.Write("|");
            for( int x = 0; x < height; x++ ){
                Console.Write(" " + PrintLetter(board[y,x]) + " |");
            }
            Console.WriteLine("");
            if(y == height-1){
                Console.Write("'");
                for( int x = 0; x < width; x++ ){ Console.Write("---'"); }
            } else {
                Console.Write("|");
                for( int x = 0; x < width; x++ ){ Console.Write("---|"); }
            }
            Console.WriteLine("");
        }
    }
}



public class HelloWorld{

    static List<string>[] dictionary;
    static int[] searches;
    // static Board board;

    static private void CreateDictionary(){
        string[] dict = System.IO.File.ReadAllLines("dictionary.txt");
        dictionary = new List<string>[30];
        for( int y = 0; y < dictionary.Length; y++ ){ dictionary[y] = new List<string>(); }
        foreach (string word in dict){ dictionary[word.Length].Add(word); }
    }


    static bool CheckWord( string word, int targetWordLength ){
        if( word.Length == targetWordLength ){
            for (int i = 0; i < dictionary[targetWordLength].Count; i++) {
                if( word == dictionary[targetWordLength][i] ) return true;
            }
        }
        return false;
    }

    static string RecursiveUtility(Board board, int v, bool[] visited, string pathSoFar, int targetLength, List<string> t_results ){
        visited[v] = true;

        string newRoute = pathSoFar + "-" + v;
        string newWord = board.ConvertRouteToWord(newRoute);
        // Console.WriteLine("checking " + newRoute + " " + newWord);

        if( CheckWord( newWord, targetLength ) ) t_results.Add( newRoute );

        if( newWord.Length <= targetLength ){
            for (int n = 0; n < board.connections[v].Count; n++) {
                int i = board.connections[v][n];
                if (!visited[i]){
                    string p = RecursiveUtility( board, i, visited, newRoute, targetLength, t_results );
                    if(p != "") return p;
                }
            }
        }

        visited[v] = false;
        return "";
    }


    static List<string> Search( Board board, int targetLength ){

        board.SetupConnections();
        // board.Print();
        List<string> t_results = new List<string>();

        // Console.WriteLine("Searching for " + targetLength + "-letter words... ");

        // start search from each square
        for (int i = 0; i < board.numberOfSquares; i++){
            if( board.getLetter(i) != "0" ){
                bool[] visited = new bool[board.numberOfSquares];
                for (int n = 0; n < board.numberOfSquares; n++) visited[n] = false;
                RecursiveUtility( board, i, visited, "", targetLength, t_results );
            }
        }

        // Console.SetCursorPosition(0, Console.CursorTop -1);
        // Console.WriteLine("Answers:                      ");

        if( t_results.Count == 0 ){
            // Console.WriteLine("No Results");
        } else {
            // for (int i = 0; i < t_results.Count; i++) {
                // Console.WriteLine( "(" + (i+1) + ") " + board.ConvertRouteToWord(t_results[i]));
            // }
        }

        return t_results;

    }


    static string Recurse( Board board, string word, string pathSoFar, int level ){

        string combinedWords = pathSoFar;
        if(level > 0 ) combinedWords += "[" + board.ConvertRouteToWord(word) + "] ";
        // if(level > 0 && level < searches.Length) combinedWords += ", ";
        // Console.WriteLine("Level " + level);

        Board b = new Board( new string[4,4]{ {"d","y","a","e"},
                                              {"r","p","s","i"},
                                              {"a","c","k","t"},
                                              {"c","r","e","n"} });
        b.Set( board );

        b.EnterWord( word );

        if( b.isClear() ){
            // Console.WriteLine("done");
            return combinedWords;
        }

        List<string> results = Search(b,searches[level]);

        for (int n = 0; n < results.Count; n++) {
            string p = Recurse( b, results[n], combinedWords, level + 1 );
            if(p != "") return p;
        }

        return "";
    }



    static public void Main (){

        searches = new int[]{4,7,5};
        CreateDictionary();
        Board board = new Board( new string[4,4]{ {"d","y","a","e"},
                                            {"r","p","s","i"},
                                            {"a","c","k","t"},
                                            {"c","r","e","n"} });


        board.Print();
        foreach( int target in searches ){
            Console.Write("[");
            for (int i = 0; i < target; i++) { Console.Write("x"); }
            Console.Write("] ");
        }
        Console.WriteLine("");
        Console.WriteLine(Recurse(board,"","",0));
    }
}

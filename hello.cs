using System;
using System.Collections.Generic;

public class Board{

    public int width;
    public int height;
    public int numberOfSquares;
    private string[,] board;

    public Board( string[,] aboard ){
        board = aboard;
        width = board.GetLength(0);
        height = board.GetLength(1);
        numberOfSquares = width * height;
    }

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

    public void setLetter( int index ){
        int y = index % width;
        int x = (int)Math.Floor( (double)index / width);
        board[x,y] = "0";
    }

    public string getLetter( int index ){
        int y = index % width;
        int x = (int)Math.Floor( (double)index / width);
        return board[x,y];
    }

    public string ConvertRouteToWord( string route ){

        string[] routes = route.Split('-');

        string word = "";

        for( int i = 0; i < routes.Length; i++ ){
            string p = routes[i];
            if( p != "" ) word = word + getLetter( (Int32.Parse(p) ) );
        }
        return word;
    }

    private string PrintLetter( string s ){
        return (s == "0" ? " " : s);
    }

    public void EnterWord( string route ){
        Console.WriteLine("Entering: " + ConvertRouteToWord( route ) );
        string[] routes = route.Split('-');
        for (int n = 0; n < routes.Length; n++) {
            string p = routes[n];
            if( p != "" ) setLetter(Int32.Parse(p));
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
    static List<int>[] connections;
    static Board board;

    static bool CheckWord( string word, int targetWordLength ){
        if( word.Length == targetWordLength ){
            for (int i = 0; i < dictionary[targetWordLength].Count; i++) {
                if( word == dictionary[targetWordLength][i] ) return true;
            }
        }
        return false;
    }

    static string RecursiveUtility(int v, bool[] visited, string pathSoFar, int targetLength, List<string> t_results ){
        visited[v] = true;

        string newRoute = pathSoFar + "-" + v;
        string newWord = board.ConvertRouteToWord(newRoute);
        // Console.WriteLine("checking " + newRoute + " " + newWord);

        if( CheckWord( newWord, targetLength ) ) t_results.Add( newRoute );

        if( newWord.Length <= targetLength ){
            for (int n = 0; n < connections[v].Count; n++) {
                int i = connections[v][n];
                if (!visited[i]){
                    string p = RecursiveUtility( i, visited, newRoute, targetLength, t_results );
                    if(p != "") return p;
                }
            }
        }

        visited[v] = false;
        return "";
    }

    static void Search( int targetLength ){

        board.Print();

        List<string> t_results = new List<string>();

        Console.WriteLine("Searching...");

        // start search from each square
        for (int i = 0; i < board.numberOfSquares; i++){
            if( board.getLetter(i) != "0" ){
                bool[] visited = new bool[board.numberOfSquares];
                for (int n = 0; n < board.numberOfSquares; n++) visited[n] = false;
                RecursiveUtility( i, visited, "", targetLength, t_results );
            }
        }

        Console.SetCursorPosition(0, Console.CursorTop -1);
        Console.WriteLine("Answers:       ");


        if( t_results.Count == 0 ){
            Console.WriteLine("No Results");
        } else {
            for (int n = 0; n < t_results.Count; n++) {
                Console.WriteLine( "(" + (n+1) + ") " + board.ConvertRouteToWord(t_results[n]));
            }
            Console.WriteLine("Enter Number of chosen Answer");

            string acceptedWord = "";
            while( acceptedWord == "" ){
                string input = Console.ReadLine();
                int n = Int32.Parse(input) - 1;
                if( n < t_results.Count ) acceptedWord = t_results[n];
            }

            board.EnterWord( acceptedWord );

            SetupSearch( board.width, board.height );
        }

    }

    static void addEdge(int node, int neighbour){
        if( neighbour >= 0 && neighbour < board.numberOfSquares ){
            bool alreadyAdded = false;
            for (int n = 0; n < connections[node].Count; n++) {
                if(connections[node][n] == neighbour) alreadyAdded = true;
            }
            if( !alreadyAdded && board.getLetter(node) != "0" && board.getLetter(neighbour) != "0" ){
                connections[node].Add(neighbour);
                connections[neighbour].Add(node);
            }
        }
    }

    static private void CreateDictionary(){
        string[] dict = System.IO.File.ReadAllLines("dictionary.txt");
        dictionary = new List<string>[30];
        for( int y = 0; y < dictionary.Length; y++ ){ dictionary[y] = new List<string>(); }
        foreach (string word in dict){ dictionary[word.Length].Add(word); }
    }

    static private void SetupSearch( int width, int height ){
        connections = new List<int>[ width * height ];

        for( int y = 0; y < board.numberOfSquares; y++ ){ connections[y] = new List<int>(); }

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

    static public void Main (){

        CreateDictionary();
        board = new Board( new string[5,5]{ {"p","o","r","t","h"},
                                            {"n","h","s","l","y"},
                                            {"u","f","i","p","i"},
                                            {"m","o","d","t","m"},
                                            {"p","l","g","l","u"} });

        SetupSearch(board.width,board.height);

        int[] searches = new int[]{5,4,8,8};
        foreach( int target in searches ){
            Search(target);
        }
    }
}

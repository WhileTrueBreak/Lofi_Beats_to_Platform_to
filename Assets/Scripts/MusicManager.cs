using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class MusicManager : MonoBehaviour
{
    private int current_id;
    private static string file_location = "./Assets/Sounds/Music/metadata.csv";
    private Song[] songList = new Song[File.ReadAllLines(@file_location).Length];
    // Start is called before the first frame update
    void Start(){
        string csv_file = readFile(file_location);

        if (csv_file == null){
            Debug.Log("Problem reading Music File");
        }

        string[] lines = csv_file.Split("\n");
        for(int i=0;i<lines.Length;i++){
            string[] fields = lines[i].Split(",");
            songList[i] = new Song(fields[0], fields[1], fields[2], fields[3], fields[4]);
        }


        // using (TextFieldParser parser = new TextFieldParser(@file_location))
        // {
        //     parser.TextFieldType = FieldType.Delimited;
        //     parser.SetDelimiters(",");
        //     while (!parser.EndOfData)
        //     {
        //         //Process row
        //         int index = 0;
        //         string[] fields = parser.ReadFields();
        //         songList[index] = new Song(fields[0], fields[1], fields[2], fields[3], fields[4]);
        //         index++;

        //     }
        // }
    }

    private string readFile(string file_location){
        using (FileStream fs = File.OpenRead(@file_location))
        {
            byte[] b = new byte[1024];
            UTF8Encoding temp = new UTF8Encoding(true);
            int readLen;
            while ((readLen = fs.Read(b,0,b.Length)) > 0)
            {
                return temp.GetString(b,0,readLen);
            }
        }
        return null;
    }
    public int getPlaying(){
        return current_id;
    }

    // public void changeSong(int songID){

    // }

}
public class Song{
        private int songID;
        public int bpm;
        public float offset;
        public string file_location;
        public string song_name;

        public Song(int songID, string song_name, int bpm, int offset, string file_location){
            this.songID = songID;
            this.bpm = bpm;
            this.offset = offset;
            this.file_location = file_location;
            this.song_name = song_name;
        }
    }

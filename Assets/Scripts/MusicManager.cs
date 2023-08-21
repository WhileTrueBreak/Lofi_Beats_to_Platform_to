using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;

public class MusicManager : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private BeatManager beatManager;

    private int current_id;
    private static string file_location = "./Assets/Sounds/Music/metadata.csv";
    private Song[] songList = new Song[File.ReadAllLines(@file_location).Length];
    // Start is called before the first frame update
    void Start(){
        audioSource = GetComponent<AudioSource>();
        initFile();
        changeSong(0);
    }

    private void initFile(){
        string csv_file = readFile(file_location);

        if (csv_file == null){
            Debug.Log("Problem reading Music File");
        }

        string[] lines = csv_file.Split("\n");
        for(int i=0;i<lines.Length;i++){
            string[] fields = lines[i].Split(",");
            songList[i] = new Song(Int32.Parse(fields[0]), fields[1], Int32.Parse(fields[2]), float.Parse(fields[3]), fields[4]);
        }
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
    public void changeSong(int songID){
        Song song = songList[songID];
        AudioClip new_clip = Resources.Load<AudioClip>(@song.file_location);
        beatManager.changeSong(song.bpm, song.offset);
    }

    public void nextSong(){
        this.changeSong(current_id + 1);
    }

}
public class Song{
        private int songID;
        public int bpm;
        public float offset;
        public string file_location;
        public string song_name;

        public Song(int songID, string song_name, int bpm, float offset, string file_location){
            this.songID = songID;
            this.bpm = bpm;
            this.offset = offset;
            this.file_location = file_location;
            this.song_name = song_name;
        }
    }

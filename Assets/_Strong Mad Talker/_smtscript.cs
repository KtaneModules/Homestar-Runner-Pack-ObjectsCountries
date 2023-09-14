﻿using Wawa.Modules;
using Wawa.Extensions;
using Wawa.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class _smtscript:ModdedModule{
    public KMSelectable[] buttons;
    public KMSelectable PSToggle;
    public Material[] PSColors;
    public TextMesh PSText;
    internal bool playing=true;
    public List<string>onesylwords;
    private string onesylword;
    public List<string>multisylwords;
    private int onesylposition;
    private string TheDecoy;
    List<int>usedWords=new List<int>();
    internal List<string>wordlist=new List<string>();
    internal List<string>fulllist=new List<string>();
    private string mantis;
    internal int stage=0;
    public GameObject[] stars;
    public Color[] buttonColors;
    private int decoyPos;
    internal settings SMTSettings;

    public sealed class settings{
        public bool SMT_LogPlayingModeInteractions=true;
        public bool SMT_LogPlayingSolvingSwitch=true;
        public bool SMT_TPResetOnTypo=false;
    }

    public static Dictionary<string,object>[]TweaksEditorSettings=new Dictionary<string,object>[]{
    new Dictionary<string,object>{
        {"Filename","strongmadtalker-settings.json"},
        {"Name","Strong Mad Talker"},
        {"Listings",new List<Dictionary<string,object>>{
            new Dictionary<string,object>{{"Key","SMT_LogPlayingModeInteractions"},{"Text","Log Interactions in Playing Mode"}},
            new Dictionary<string,object>{{"Key","SMT_LogPlayingSolvingSwitch"},{"Text","Log Switching Between Playing and Solving Modes"}},
            new Dictionary<string,object>{{"Key","SMT_TPResetOnTypo"},{"Text","Twitch Plays: Reset Input on Typo"}}
            }
        }}
    };

    protected override void OnActivate(){
        foreach(KMSelectable button in buttons){
            button.Add(onHighlight:()=>button.GetComponentInChildren<TextMesh>().color=buttonColors[1],
                onHighlightEnded:()=>button.GetComponentInChildren<TextMesh>().color=buttonColors[0],
                onInteract:()=>{Press(button);button.GetComponentInChildren<TextMesh>().color=buttonColors[2];},
                onInteractEnded:()=>button.GetComponentInChildren<TextMesh>().color=buttonColors[1]);
        }
        PSToggle.Set(onInteract:PS);
    }

    void Start(){
        SMTSettings=new Config<settings>("strongmadtalker-settings.json").Read();
        foreach(GameObject star in stars)star.SetActive(false);
        onesylposition=UnityEngine.Random.Range(0,6);
        PlaceWords();
        foreach(KMSelectable button in buttons)
            button.GetComponentInChildren<TextMesh>().fontSize=resize(button.GetComponentInChildren<TextMesh>().text);
        TheDecoy=Decoy(OrderCheck(onesylword));
        Log("{0} is the decoy.",TheDecoy);
        if(TheDecoy=="MANTIS")pressingOrder(mantis);
        else pressingOrder(TheDecoy);
    }

    internal void PS(){
        if(Status.IsSolved)
            return;
        playing=!playing;
        if(playing){
            PSToggle.GetComponent<Renderer>().material=PSColors[0];
            PSText.text="Playing";
            if(!Status.IsSolved){
                stage=0;
                foreach(GameObject star in stars)star.SetActive(false);
            }
        }else{
            PSToggle.GetComponent<Renderer>().material=PSColors[1];
            PSText.text="Solving";
        }
        if(SMTSettings.SMT_LogPlayingSolvingSwitch)Log("Switched to {0} mode.",PSText.text);
    }

    void PlaceWords(){
        for(int i=0;i<6;i++){
            if(i!=onesylposition){
                int wordPos=UnityEngine.Random.Range(0,12);
                while(usedWords.Contains(wordPos))wordPos=UnityEngine.Random.Range(0,12);
                usedWords.Add(wordPos);
                buttons[i].GetComponentInChildren<TextMesh>().text=multisylwords[wordPos];
            }else{
                onesylword=onesylwords[UnityEngine.Random.Range(0,4)];
                buttons[i].GetComponentInChildren<TextMesh>().text=onesylword;
            }
        }
    }

    List<int>OrderCheck(string word){
        if(word=="PULSE")return new List<int>{0,2,4,1,3,5};
        if(word=="CAKE")return new List<int>{4,3,0,1,2,5};
        if(word=="CHARM")return new List<int>{3,2,0,1,5,4};
        return new List<int>{0,1,2,3,4,5};
    }

    string Decoy(List<int>Order){
        for(int i=0;i<6;i++){
            wordlist.Add(buttons[i].GetComponentInChildren<TextMesh>().text);
            fulllist.Add(buttons[i].GetComponentInChildren<TextMesh>().text);
        }
        Log("The words are, in reading order: {0}",string.Join(", ",wordlist.ToArray()));
        for(int i=0;i<6;i++){
            string word=buttons[Order[i]].GetComponentInChildren<TextMesh>().text;
            decoyPos=DecoyChecking(word,Order[i],i);
            if(decoyPos!=6){
                if(decoyPos==onesylposition){
                    if(onesylposition%2==0)
                        return mantising(buttons[(onesylposition+1)%6].GetComponentInChildren<TextMesh>().text,word,false);
                    else return mantising(buttons[(onesylposition-1)%6].GetComponentInChildren<TextMesh>().text,word,false);
                }
                return mantising(buttons[decoyPos%6].GetComponentInChildren<TextMesh>().text,word,false);
            }
        }
        Log("None of the conditions were true.");
        if(onesylposition%2==0)return mantising(buttons[(onesylposition+1)%6].GetComponentInChildren<TextMesh>().text,"SWEETYCAKES",true);
        return mantising(buttons[(onesylposition-1)%6].GetComponentInChildren<TextMesh>().text,"SWEETYCAKES",true);
    }

    string mantising(string mantistest,string word,bool sweetyfakes){
        if(mantistest=="MANTIS"){
            mantis=word;
            //wordlist defaults to reading order so i didn't need to specifically use "SWEETYCAKES" but. come on. how could i pass up this pun
            if(sweetyfakes)Log("Because MANTIS is the decoy and it was not declared the decoy by another word, the buttons must be pressed in reading order.");
            else Log("Because MANTIS is the decoy, the order from the word that declared it the decoy will be used, which is {0}.",mantis);
            decoyPos=wordlist.IndexOf("MANTIS");
        } return mantistest;
    }

    int DecoyChecking(string word,int currentPos,int count){
        switch(word){
            case"SWEETYCAKES":return Get<KMBombInfo>().GetOnIndicators().ToArray().Length>=2?1:6;
            case"CHEDDAR":return (usedWords.Contains(0)||usedWords.Contains(10)||onesylword=="CAKE"||onesylword=="JUICE")?currentPos:6;
            case"DOUGLAS":return buttons[0].GetComponentInChildren<TextMesh>().text.Length
                               >=buttons[5].GetComponentInChildren<TextMesh>().text.Length
                               ? 6 : OrderCheck(onesylword)[Get<KMBombInfo>().GetSerialNumberNumbers().Sum()%6];
            case"GARBLEDINA":return currentPos;
            case"MANTIS":
                     if(currentPos%2==0&&multisylwords.Take(6).Contains(buttons[currentPos+1].GetComponentInChildren<TextMesh>().text))
                        return currentPos+1;
                else if(currentPos%2==1&&multisylwords.Take(6).Contains(buttons[currentPos-1].GetComponentInChildren<TextMesh>().text))
                        return currentPos-1;
                else return 6;
            case"DIAPER":return count>=2&&count<=4?2:6;
            case"PROXIMITY":return
                      ((onesylposition<4&&currentPos==onesylposition+2)
                     ||(onesylposition>1&&currentPos==onesylposition-2)
                     ||(currentPos%2==0&&currentPos==onesylposition-1)
                     ||(currentPos%2==1&&currentPos==onesylposition+1))?5:6;
            case"MOVIE":return Get<KMBombInfo>().GetSerialNumberLetters().Any(onesylword.Contains)?(currentPos+4)%6:6;
            case"HORSES":return onesylposition%2==0?OrderCheck(onesylword)[3]:6;
            case"WORKING":return buttons[0].GetComponentInChildren<TextMesh>().text.Length
                               ==buttons[4].GetComponentInChildren<TextMesh>().text.Length
                                 ?2:6;
            case"CASSEROLE":
                if((currentPos+2)%6!=onesylposition)
                    for(int i=0;i<count;i++)
                        if((currentPos+2)%6==OrderCheck(onesylword)[i])
                            return currentPos;
                return 6;
            case"AROUND":if(usedWords.Contains(0)||usedWords.Contains(3)){
                    if(count==0||(count==1&&OrderCheck(onesylword)[0]==onesylposition))
                        return currentPos;
                    if(OrderCheck(onesylword)[count-1]==onesylposition)
                        return OrderCheck(onesylword)[count-2];
                    return OrderCheck(onesylword)[count-1];
                }return 6;
            default:return 6;
        }
    }

    internal void Press(KMSelectable button){
        Play(Sound.BigButtonPress);
        Play(new Sound(button.GetComponentInChildren<TextMesh>().text));
        button.AddInteractionPunch();
        string word=button.GetComponentInChildren<TextMesh>().text;
        string message="Pressed "+word;
        if(Status.IsSolved||playing){
            if(SMTSettings.SMT_LogPlayingModeInteractions)Log(message+" in Playing mode.");
            return;
        }
        message+=", which was ";
        if(word==wordlist[stage]){
            Log(message+"correct.");
            stars[stage].SetActive(true);
            stage++;
            if(stage==4){
                Solve("SOLVED!");
                PS();
                foreach(GameObject star in stars)star.SetActive(true);
            } return;
        }
        if(word==onesylword)Log(message+"the one-syllable word.");
        else if(word==TheDecoy)Log(message+"the decoy.");
        else Log(message+"incorrect.");
        wrong();
    }
    int resize(string word){
        switch(word){
            case "MOTORCYCLES":
            case "SWEETYCAKES":
                return 16;
            case "BATTLESHIP":
            case "CASSEROLE":
            case "GARBLEDINA":
            case "PROXIMITY":
            case "TRAINWRECK":
                return 20;
            case "DELOUISE":
            case "WORKING":
                return 24;
            case "AROUND":
            case "DOUGLAS":
            case "MANTIS":
                return 26;
            case "CHEDDAR":
            case "DIAPER":
            case "HORSES":
            case "MOVIE":
                return 28;
            default:
                return 30;
        }
    }
    void wrong(){
        Strike("STRIKE!");
        stage=0;
        foreach(GameObject star in stars)star.SetActive(false);
    }
    void pressingOrder(string decoy){
        List<int>specialList=new List<int>();
        if(decoy=="HORSES")specialList=new List<int>{5,4,3,1,0,2};
        if(decoy=="AROUND")specialList=new List<int>{0,1,3,5,4,2};
        if(decoy=="CASSEROLE")specialList=OrderCheck(onesylword);
        if(decoy=="GARBLEDINA"){
            List<int>rearrange=new List<int>{1,5,3,4,2,0};
            wordlist.Clear();
            for(int i=0;i<6;i++)wordlist.Add(buttons[OrderCheck(onesylword)[rearrange[i]]].GetComponentInChildren<TextMesh>().text);
        }
        if(decoy=="MOVIE"){
            specialList=new List<int>{0,1,3,5,4,2};
            int offset=specialList.IndexOf(decoyPos);
            List<int>clockwise=new List<int>();
            for(int i=0;i<6;i++)clockwise.Add(specialList[(i+offset)%6]);
            specialList=clockwise;
        }
        if(specialList.ToArray().Length!=0){
            wordlist.Clear();
            for(int i=0;i<6;i++)wordlist.Add(buttons[specialList[i]].GetComponentInChildren<TextMesh>().text);
        }
        wordlist.Remove(TheDecoy);
        wordlist.Remove(onesylword);
        if(decoy=="PROXIMITY")wordlist.Sort((x,y)=>string.Compare(x,y));
        if(decoy=="WORKING"){
            wordlist.Sort((x,y)=>string.Compare(x,y));
            wordlist.Reverse();
        }
        if(decoy=="CHEDDAR"||decoy=="DIAPER"){
            wordlist.Sort((x,y)=>string.Compare(x,y));
            wordlist=wordlist.OrderBy(x =>x.Length).ToList();
            if(decoy=="DIAPER"){
                List<string>templist=new List<string>();
                for(int i=0;i<4;i++)templist.Add(wordlist[(5-i)%4]);
                wordlist=templist;
            }
        }
        if(decoy=="DOUGLAS"){
            List<string>templist=new List<string>();
            for(int i=6;i<18;i++)if(wordlist.Contains(multisylwords[i%12]))templist.Add(multisylwords[i%12]);
            wordlist=templist;
        }Log("Correct order: {0}",string.Join(", ",wordlist.ToArray()));
    }
}
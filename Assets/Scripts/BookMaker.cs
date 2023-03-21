using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.Networking;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using System;

public class BookMaker : MonoBehaviour
{
    // A BookShelf
    public BookShelf bookShelf;

    public TMP_Dropdown availableBooks;

    public GameObject physicalBook;

    // Variables for displaying the report.
    public TMPro.TextMeshProUGUI reportStatistics;
    public TMP_Dropdown wrongWords;
    private Stopwatch stopwatch;
    private bool firstStartAttempt = true;

    public bool isPracticeMode = false;

    // Variables for displaying the book pages.
    public TMPro.TextMeshProUGUI currentSentence;
    public TMPro.TextMeshProUGUI heardSentence;
    public int currentSentenceNumber = 0;
    public int totalNumberSentences = 0;
    public Book currentBook;
    public string currentBookTitle;
    private string[] heardSentences;

    // Variables for passing to next level in story mode
    public float fadeTime = 2.0f; // Time it takes for the object to fade out
    private float currentFadeTime = 0.0f; // Current time the object has been fading
    public bool passedLevel = false;
    public GameObject door;


    public void setCurrentBookTitle(TMP_Dropdown optionMenu)
    {
        currentBookTitle = optionMenu.options[optionMenu.value].text;
        setCurrentBook(currentBookTitle);
    }

    public void setCurrentBookTitle(string title)
    {
        currentBookTitle = title;
        setCurrentBook(title);
    }

    public void setCurrentBook(string title)
    {
        currentBook = bookShelf.getBookByName(title);
        totalNumberSentences = currentBook.numPages;
        currentSentenceNumber = 0;
        heardSentences = new string[totalNumberSentences];
        for (int i = 0; i < totalNumberSentences; i++)
        {
            heardSentences[i] = "";
        }
        stopwatch.Reset();
        firstStartAttempt = true;
        displayCurrentBook();
    }

    public void displayCurrentBook()
    {
        if (currentSentenceNumber < totalNumberSentences)
        {
            currentSentence.text = currentBook.text[currentSentenceNumber];
            heardSentence.text = heardSentences[currentSentenceNumber];
        }
    }

    public void nextSentenceButton()
    {
        if (currentSentenceNumber == totalNumberSentences)
        {
            stopwatch.Stop();
            createReport();
        }
        if (currentSentenceNumber < totalNumberSentences)
        {
            heardSentences[currentSentenceNumber] = heardSentence.text;
            currentSentenceNumber++;
            if (currentSentenceNumber < totalNumberSentences)
            {
                heardSentence.text = heardSentences[currentSentenceNumber];
            }
            displayCurrentBook();
        }
    }

    public void lastSentenceButton()
    {
        if (currentSentenceNumber > 0)
        {
            heardSentences[currentSentenceNumber] = heardSentence.text;
            currentSentenceNumber--;
            displayCurrentBook();
        }
    }

    public void createReport()
    {
        Report bookReport = new Report(currentBook.text, heardSentences.ToList(), stopwatch.Elapsed);
        currentBook.resultReport = bookReport;
        displayReport();
    }

    private void displayReport()
    {
        float percentage = ((float)currentBook.resultReport.numCorrectWords / (float)currentBook.resultReport.numWords) * 100.00f;
        string report = "Percent Correct: " + percentage.ToString("0.0000") + "%\n"
                        + "Words Correct: " + currentBook.resultReport.numCorrectWords + "\n"
                        + "Words Incorrect: " + currentBook.resultReport.numIncorrectWords + "\n"
                        + "Completion Time: " + currentBook.resultReport.completionTime;
        reportStatistics.text = report;
        wrongWords.ClearOptions();
        List<string> incorrectWords = new List<string> {"Incorrect Words"};
        incorrectWords.AddRange(currentBook.resultReport.incorrectWords);
        wrongWords.AddOptions(incorrectWords);
        physicalBook.GetComponent<CognitiveReader>().TextToSpeechActivate("Report Generated");
        if (!isPracticeMode && percentage > 50.00f)
        {
            passedLevel = true;
        }
    }

    public void startStopwatch()
    {
        if (firstStartAttempt)
        {
            stopwatch.Start();
            firstStartAttempt = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        bookShelf = new BookShelf();
        stopwatch = new Stopwatch();
        GetFiles();
        setCurrentBook(currentBookTitle);

        if (availableBooks != null)
        {
            availableBooks.ClearOptions();
            availableBooks.AddOptions(bookShelf.GetBookTitles());
        }
    }

    // Update method
    void Update()
    {
        if (!isPracticeMode && passedLevel)
        {
            currentFadeTime += Time.deltaTime;
            float alpha = 1.0f - Mathf.Clamp01(currentFadeTime / fadeTime);
            Color color = door.GetComponent<Renderer>().material.color;
            color.a = alpha;
            door.GetComponent<Renderer>().material.color = color;

            if (currentFadeTime >= fadeTime)
            {
                door.SetActive(false);
            }

        }
    }

    // Gets all files in the Practice folder
    public void GetFiles()
    {
        //Debug.Log(Application.streamingAssetsPath);
        //Debug.Log(System.IO.Directory.GetFiles(Application.streamingAssetsPath)[0]);
        // Get all files in the Practice folder
        if (isPracticeMode)
        {
            string[] files = System.IO.Directory.GetFiles(Application.streamingAssetsPath + "/Stories/PracticeStories");

            // For each file in the Practice folder
            foreach (string file in files)
            {
                // Get the file name
                string fileName = System.IO.Path.GetFileName(file);

                // If the file is a .txt file
                if (fileName.EndsWith(".txt"))
                {
                    UnityEngine.Debug.Log(fileName);
                    MakeBook(fileName);
                }
            }
        } else
        {
            string[] files = System.IO.Directory.GetFiles(Application.streamingAssetsPath + "/Stories/MainStory");

            // For each file in the Main Story folder
            foreach (string file in files)
            {
                // Get the file name
                string fileName = System.IO.Path.GetFileName(file);

                // If the file is a .txt file
                if (fileName.EndsWith(".txt"))
                {
                    MakeBook(fileName);
                }
            }
        }
    }

    // Make a book
    public void MakeBook(string fileName)
    {
        char[] delimiterChars = {'?', '.', '!'};

        string text = "";

        if (isPracticeMode)
        {
            // Reads the text file at the Assets/Stories/PracticeStories/fileName path
            text = System.IO.File.ReadAllText(Application.streamingAssetsPath + "/Stories/PracticeStories/" + fileName);
        } else
        {
            text = System.IO.File.ReadAllText(Application.streamingAssetsPath + "/Stories/MainStory/" + fileName);
        }

        //string[] sentences = text.Split(delimiterChars).Select(x => x.Trim()).ToArray();
        List<string> sentences = new List<string>(Regex.Split(text, @"(?<=[.]|[?]|[!])"));

        // Remove all whitespace only or empty string items from the list.
        sentences.RemoveAll(x => (string.Concat(x.Where(c => !char.IsWhiteSpace(c)))) == "");

        // For each string in "sentences", Trim the whitespace.
        for (int i = 0; i < sentences.Count; i++)
        {
            sentences[i] = sentences[i].Trim();
        }

        // Create a new book
        Book book = new Book(RemoveTXT(fileName), sentences);
        bookShelf.AddBook(book);

    }

    // A function for removing the .txt file type from the passed string
    public string RemoveTXT(string fileName)
    {
        string extensionToRemove = ".txt";
        string name = fileName.Replace(extensionToRemove, string.Empty);
        return name;
    }



    // A Class Representing a Bookshelf
    public class BookShelf
    {
        // A Dictionary of Books with the key being the book's name
        public Dictionary<string, Book> books = new Dictionary<string, Book>();

        // A Method to Add a Book to the Shelf
        public void AddBook(Book book)
        {
            books.Add(book.Title, book);
        }

        // A Method to Remove a Book from the Shelf
        public void RemoveBook(Book book)
        {
            books.Remove(book.Title);
        }

        // A Method to Return a list of the Titles of the Books on the Shelf
        public List<string> GetBookTitles()
        {
            List<string> bookTitles = new List<string>();

            foreach (KeyValuePair<string, Book> book in books)
            {
                bookTitles.Add(book.Key);
            }

            return bookTitles;
        }

        // A Method to return a book based on the name
        public Book getBookByName(string name) => books[name];
    }

    // A Class Representing a Book
    public class Book
    {
        // Properties
        public string Title { get; set; }
        public List<string> text { get; set; }
        public int numPages { get; set; }
        public int numWords { get; set; }
        public Report resultReport { get; set; }

        // Constructor
        public Book(string title, List<string> text)
        {
            this.Title = title;
            this.text = text;
            this.numPages = text.Count;
            this.numWords = 0;
            foreach (string page in text)
            {
                this.numWords += page.Split(' ').Length;
            }
            this.resultReport = null;
        }
    }

    // A Class Representing a Report
    public class Report
    {
        // Properties
        public List<string> sourceText { get; set; }
        public List<string> recordedText { get; set; }
        public TimeSpan timeToComplete { get; set; }

        public List<string> correctWords { get; set; }
        public List<string> incorrectWords { get; set; }

        public int numWords { get; set; }
        public int numCorrectWords { get; set; }
        public int numIncorrectWords { get; set; }
        public string completionTime { get; set; }

        // Constructor
        public Report(List<string> sourceText, List<string> recordedText, TimeSpan timeToComplete)
        {
            this.sourceText = sourceText;
            this.recordedText = recordedText;
            this.timeToComplete = timeToComplete;
            this.correctWords = new List<string>();
            this.incorrectWords = new List<string>();
            foreach (string sentence in this.sourceText)
            {
                this.numWords += sentence.Split(' ').Length;
            }
            //this.numWords = 0;
            this.numCorrectWords = 0;
            this.numIncorrectWords = 0;
            // Format and display the TimeSpan value.
            this.completionTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                timeToComplete.Hours, timeToComplete.Minutes, timeToComplete.Seconds,
                timeToComplete.Milliseconds / 10);
            // For each page in the source text and the recorded text (they should be the same length)
            // compare the words and add them to the correct or incorrect lists
            for (int i = 0; i < sourceText.Count; i++)
            {
                CompareText(sourceText[i], recordedText[i]);
            }
        }

        // A Method to Compare the Source Text and the Recorded Text
        public void CompareText(string string1, string string2) {
            // Remove punctuation from both strings
            string1 = new string(string1.Where(c => !char.IsPunctuation(c)).ToArray());
            string2 = new string(string2.Where(c => !char.IsPunctuation(c)).ToArray());

            // Split the strings into arrays of words
            string[] words1 = string1.ToLower().Split(' ');
            string[] words2 = string2.ToLower().Split(' ');

            ////int shorterLength = words1.Length <= words2.Length ? words1.Length : words2.Length;

            //// Compare the arrays word by word
            //for (int i = 0; i < words1.Length; i++)
            //{
            //    if (i >= words2.Length)
            //    {
            //        for (int j = i; j < words1.Length; j++)
            //        {
            //            incorrectWords.Add(words1[j]);
            //        }
            //        break;
            //    }
            //    //numWords++;
            //    UnityEngine.Debug.Log(string1);
            //    UnityEngine.Debug.Log(string2);
            //    if (words1[i] != words2[i])
            //    {
            //        incorrectWords.Add(words1[i]);
            //        numIncorrectWords++;
            //    }
            //    else
            //    {
            //        correctWords.Add(words1[i]);
            //        numCorrectWords++;
            //    }
            //}

            List<string> missedWords = words1.Except(words2).ToList();
            incorrectWords.AddRange(missedWords);
            numIncorrectWords += missedWords.Count;
            List<string> foundWords = words1.Except(missedWords).ToList();
            correctWords.AddRange(foundWords);
            numCorrectWords += foundWords.Count;
        }
    }
}

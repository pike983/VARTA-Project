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
using System.Globalization;

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

    // Sets the current book to the book selected in the dropdown menu.
    public void setCurrentBookTitle(TMP_Dropdown optionMenu)
    {
        currentBookTitle = optionMenu.options[optionMenu.value].text;
        setCurrentBook(currentBookTitle);
    }

    // Sets the current book to the book title provided in the string.
    public void setCurrentBookTitle(string title)
    {
        currentBookTitle = title;
        setCurrentBook(title);
    }

    // Sets the current book to the book title provided in the string and starts the display of the book.
    // This is the function that should be called when the user selects a book to read.
    // It sets up the variables required to keep track of the user progress through the book.
    // This includes preparing a vector to hold heard sentences, and reseting the stopwatch and sentence number.
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
        // Used to tell when to start the stopwatch.
        firstStartAttempt = true;
        displayCurrentBook();
    }

    // Displays the current book at the current sentence.
    // Also displays the sentence that the user spoke previously if it exists, otherwise a blank string.
    public void displayCurrentBook()
    {
        if (currentSentenceNumber < totalNumberSentences)
        {
            currentSentence.text = currentBook.text[currentSentenceNumber];
            heardSentence.text = heardSentences[currentSentenceNumber];
        }
    }

    // Displays the next sentence in the book.
    // Also displays the sentence that the user spoke previously if it exists, otherwise a blank string.
    // Also stops the stopwatch if the user has reached the end of the book and starts the report generating.
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
                // heardSentence.text = heardSentences[currentSentenceNumber];
                physicalBook.GetComponent<CognitiveReader>().ReplaceHeardMessage(heardSentences[currentSentenceNumber]);
            }
            displayCurrentBook();
        }
    }

    // Displays the previous sentence in the book.
    // Also displays the sentence that the user spoke previously if it exists, otherwise a blank string.
    public void lastSentenceButton()
    {
        if (currentSentenceNumber > 0)
        {
            heardSentences[currentSentenceNumber] = heardSentence.text;
            currentSentenceNumber--;
            physicalBook.GetComponent<CognitiveReader>().ReplaceHeardMessage(heardSentences[currentSentenceNumber]);
            displayCurrentBook();
        }
    }
    
    // Starts the creation of the user results report from reading the book.
    public void createReport()
    {
        Report bookReport = new Report(currentBook.text, heardSentences.ToList(), stopwatch.Elapsed);
        currentBook.resultReport = bookReport;
        displayReport();
    }

    // Displays the user results report from reading the book.
    // This includes the percentage of words read correctly, the number of words read correctly, and the number of words read incorrectly.
    // It also displays the time it took to read the book.
    // If the user is in the story mode, it removes the door blocking the next level.
    // This door is only removed when the user has read at least 50% of the words correctly.
    // The door is not present in practice mode.
    // The door is removed by setting the boolean passedLevel to true.
    // This boolean is checked in the Update function of this script.
    private void displayReport()
    {
        float percentage = ((float)currentBook.resultReport.numCorrectWords / (((float)currentBook.resultReport.numCorrectWords) + (float)currentBook.resultReport.numIncorrectWords)) * 100.00f;
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

    // Starts a stopwatch to keep track of the time it takes the user to read the book.
    public void startStopwatch()
    {
        if (firstStartAttempt)
        {
            stopwatch.Start();
            firstStartAttempt = false;
        }
    }

    // A function to create the stopwatch.
    public void MakeStopWatch()
    {
        stopwatch = new Stopwatch();
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
            // Add the available books to the dropdown menu.
            availableBooks.ClearOptions();
            availableBooks.AddOptions(bookShelf.GetBookTitles());
        }
    }

    // Update method
    void Update()
    {
        // If the user has passed the level, fade out the door and set it inactive.
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

    // Gets all book files from the Streaming Assets folder.
    public void GetFiles()
    {
        // Get all files in the Practice folder
        if (isPracticeMode)
        {
            string[] files = System.IO.Directory.GetFiles(Application.streamingAssetsPath + "/Stories/PracticeStories");

            // For each file in the Practice folder
            foreach (string file in files)
            {
                // Get the file name
                string fileName = System.IO.Path.GetFileName(file);

                // If the file is a .txt file, create the book.
                if (fileName.EndsWith(".txt"))
                {
                    MakeBook(fileName);
                }
            }
        } else // Get all files in the Main Story folder.
        {
            string[] files = System.IO.Directory.GetFiles(Application.streamingAssetsPath + "/Stories/MainStory");

            // For each file in the Main Story folder
            foreach (string file in files)
            {
                // Get the file name
                string fileName = System.IO.Path.GetFileName(file);

                // If the file is a .txt file, create the book.
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
        // The characters to split sentences by.
        char[] delimiterChars = {'?', '.', '!'};

        string text = "";

        if (isPracticeMode)
        {
            // Reads the text file at the Assets/Stories/PracticeStories/fileName path
            text = System.IO.File.ReadAllText(Application.streamingAssetsPath + "/Stories/PracticeStories/" + fileName);
        } else
        {
            // Reads the text file at the Assets/Stories/MainStory/fileName path
            text = System.IO.File.ReadAllText(Application.streamingAssetsPath + "/Stories/MainStory/" + fileName);
        }

        // Splits the text into sentences, keeping the punctuation using Regex.
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

    // A function for removing the .txt file type from the passed string.
    // Used for creating the book titles that can be displayed in the game to choose from.
    public string RemoveTXT(string fileName)
    {
        TextInfo textInfo = new CultureInfo("en-CA", false).TextInfo;
        string extensionToRemove = ".txt";
        string name = fileName.Replace(extensionToRemove, string.Empty);
        name = textInfo.ToTitleCase(name);
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

            // Finds the words that were spoken incorrectly by finding the words that are in the source text
            // but that are not in the recorded text.
            List<string> missedWords = words1.Except(words2).ToList();
            incorrectWords.AddRange(missedWords);
            numIncorrectWords += missedWords.Count;

            // Finds the words that were spoken correctly by finding the words from the source text
            // that were determined to not be missing.
            List<string> foundWords = words1.Except(missedWords).ToList();
            correctWords.AddRange(foundWords);
            numCorrectWords += foundWords.Count;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System;

public class BookMakerTests
{
    //// A Test behaves as an ordinary method
    //[Test]
    //public void BookMakerTestsSimplePasses()
    //{
    //    // Use the Assert class to test conditions
    //}

    //// A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    //// `yield return null;` to skip a frame.
    //[UnityTest]
    //public IEnumerator BookMakerTestsWithEnumeratorPasses()
    //{
    //    // Use the Assert class to test conditions.
    //    // Use yield to skip a frame.
    //    yield return null;
    //}
    
    private GameObject createBookMakerObject()
    {
        // Create a game object and add the BookMaker script to it.
        GameObject testObject = new GameObject();
        BookMaker bM = testObject.AddComponent<BookMaker>();

        // Create a canvas and add another object with a TextMeshProUGUI component to it.
        GameObject canvas = new GameObject("Canvas");
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(canvas.transform);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();

        // Create a game object and add a TMP_Dropdown to it, then add the object to the canvas.
        GameObject dropdownObject = new GameObject("Dropdown");
        dropdownObject.transform.SetParent(canvas.transform);
        TMP_Dropdown dropdown = dropdownObject.AddComponent<TMP_Dropdown>();

        // Create a game object and add a CognitiveReader to it
        GameObject cognitiveReaderObject = new GameObject("CognitiveReader");
        CognitiveReader cg = cognitiveReaderObject.AddComponent<CognitiveReader>();

        // Create a game object and add a TMP_Dropdown to it, then add the object to the canvas.
        GameObject dropdownObject2 = new GameObject("Dropdown");
        dropdownObject2.transform.SetParent(canvas.transform);
        TMP_Dropdown dropdown2 = dropdownObject2.AddComponent<TMP_Dropdown>();

        // Create a game object and add a TextMeshProUGUI to it, then add the object to the canvas.
        GameObject textObject2 = new GameObject("Text");
        textObject2.transform.SetParent(canvas.transform);
        TextMeshProUGUI text2 = textObject2.AddComponent<TextMeshProUGUI>();

        // Create a game object and add a TextMeshProUGUI to it, then add the object to the canvas.
        GameObject textObject3 = new GameObject("Text");
        textObject3.transform.SetParent(canvas.transform);
        TextMeshProUGUI text3 = textObject3.AddComponent<TextMeshProUGUI>();

        // Create a cube game object
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // Create a canvas and add another object with a TextMeshProUGUI component to it.
        GameObject canvas2 = new GameObject("Canvas");
        GameObject textObject4 = new GameObject("Text");
        textObject4.transform.SetParent(canvas2.transform);
        TextMeshProUGUI text4 = textObject4.AddComponent<TextMeshProUGUI>();

        // Create another object with a TextMeshProUGUI component to it and add it to canvas2.
        GameObject textObject5 = new GameObject("Text");
        textObject5.transform.SetParent(canvas2.transform);
        TextMeshProUGUI text5 = textObject5.AddComponent<TextMeshProUGUI>();

        // Create a game object and add a button to it, then add the object to the canvas.
        GameObject buttonObject = new GameObject("Button");
        buttonObject.transform.SetParent(canvas2.transform);
        Button button = buttonObject.AddComponent<Button>();

        // Create a game object and add a audio source to it.
        GameObject audioObject = new GameObject("Audio");
        audioObject.AddComponent<AudioSource>();
        
        cg.outputText = text4;
        cg.recognizedOutputText = text5;
        cg.startRecoButton = button;
        cg.audioSource = audioObject.GetComponent<AudioSource>();

        bM.availableBooks = dropdown;
        bM.physicalBook = cognitiveReaderObject;
        bM.reportStatistics = text;
        bM.wrongWords = dropdown2;
        bM.currentSentence = text2;
        bM.heardSentence = text3;
        bM.door = cube;
        bM.currentBookTitle = "The Bus And New Friends";
        bM.isPracticeMode = true;

        // Set the bm variable bookShelf to a new BookShelf class from the BookMaker script.
        bM.bookShelf = new BookMaker.BookShelf();
        bM.MakeStopWatch();
        return testObject;
    }

    // Test to see if the book is changed when the setCurrentBookTitle function is called.
    [Test]
    [TestCase("The Bus And New Friends")]
    [TestCase("The Mouse And The Stolen Nuts")]
    [TestCase("The Keys")]
    [TestCase("The Bus And New Friends")]
    public void setCurrentBookTitleTest(string title)
    {
        // Create a game object and add the BookMaker script to it.
        GameObject testObject = createBookMakerObject();
        BookMaker bM = testObject.GetComponent<BookMaker>();

        bM.GetFiles();
        bM.setCurrentBook("The Bus And New Friends");

        // Call the setCurrentBookTitle function
        bM.setCurrentBookTitle(title);

        // Check to see if the book is changed.
        Assert.AreEqual(title, bM.currentBook.Title);
    }

    // Test of the GetFiles function for retrieving the correct number of books.
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void GetFilesTest_RetrievesProperNumberOfBooks(bool practiceMode)
    {
        // Create a game object and add the BookMaker script to it.
        GameObject testObject = createBookMakerObject();
        BookMaker bM = testObject.GetComponent<BookMaker>();
        bM.isPracticeMode = practiceMode;

        // Call the GetFiles function.
        bM.GetFiles();

        if (practiceMode)
        {
            // Check to see if the bookshelf has 3 practice books.
            Assert.IsTrue(bM.bookShelf.books.Count == 3);
        }
        else
        {
            // Check to see if the bookshelf has 3 story books.
            Assert.IsTrue(bM.bookShelf.books.Count == 3);
        }
    }

    // Test of the GetFiles function for retrieving the correct files.
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void GetFilesTest_RetrievesProperBooks(bool practiceMode)
    {
        // Create a game object and add the BookMaker script to it.
        GameObject testObject = createBookMakerObject();
        BookMaker bM = testObject.GetComponent<BookMaker>();
        bM.isPracticeMode = practiceMode;

        // Call the GetFiles function.
        bM.GetFiles();

        if (practiceMode)
        {
            Assert.IsTrue(bM.bookShelf.books.ContainsKey("The Bus And New Friends"));
            Assert.IsTrue(bM.bookShelf.books.ContainsKey("The Mouse And The Stolen Nuts"));
            Assert.IsTrue(bM.bookShelf.books.ContainsKey("The Keys"));
        }
        else
        {
            Assert.IsTrue(bM.bookShelf.books.ContainsKey("Main Story Act One"));
            Assert.IsTrue(bM.bookShelf.books.ContainsKey("Main Story Act Two"));
            Assert.IsTrue(bM.bookShelf.books.ContainsKey("Main Story Act Three"));
        }
    }

    // Test of the RemoveTXT function for removing the .txt extension from a string.
    [Test]
    [TestCase("The Bus And New Friends.txt")]
    [TestCase("The Mouse And The Stolen Nuts.txt")]
    [TestCase("The Keys.txt")]
    [TestCase("Main Story Act One.txt")]
    [TestCase("Main Story Act Two.txt")]
    [TestCase("Main Story Act Three.txt")]
    public void RemoveTXTTest(string title)
    {
        // Create a game object and add the BookMaker script to it.
        GameObject testObject = createBookMakerObject();
        BookMaker bM = testObject.GetComponent<BookMaker>();

        bM.GetFiles();
        bM.setCurrentBook("The Bus And New Friends");

        // Call the RemoveTXT function.
        string result = bM.RemoveTXT(title);

        // Check to see if the .txt extension is removed.
        Assert.IsFalse(result.Contains(".txt"));
    }

    // Test of the RemoveTXT function for removing the .txt extension from a string with no .txt extension.
    [Test]
    [TestCase("The Bus And New Friends")]
    [TestCase("The Mouse And The Stolen Nuts")]
    [TestCase("The Keys")]
    [TestCase("Main Story Act One")]
    [TestCase("Main Story Act Two")]
    [TestCase("Main Story Act Three")]
    public void RemoveTXTTest_NoExtension(string title)
    {
        // Create a game object and add the BookMaker script to it.
        GameObject testObject = createBookMakerObject();
        BookMaker bM = testObject.GetComponent<BookMaker>();

        bM.GetFiles();
        bM.setCurrentBook("The Bus And New Friends");
        
        // Call the RemoveTXT function.
        string result = bM.RemoveTXT(title);

        // Check to see if the .txt extension is removed.
        Assert.IsFalse(result.Contains(".txt"));
    }

    // Test of the RemoveTXT function for different file types.
    [Test]
    [TestCase("The Bus And New Friends.txt")]
    [TestCase("The Mouse And The Stolen Nuts.txt")]
    [TestCase("The Keys.txt")]
    [TestCase("Main Story Act One.txt")]
    [TestCase("Main Story Act Two.txt")]
    [TestCase("Main Story Act Three.txt")]
    [TestCase("The Bus And New Friends")]
    [TestCase("The Mouse And The Stolen Nuts")]
    [TestCase("The Keys")]
    [TestCase("Main Story Act One")]
    [TestCase("Main Story Act Two")]
    [TestCase("Main Story Act Three")]
    [TestCase("The Bus And New Friends.pdf")]
    [TestCase("The Mouse And The Stolen Nuts.docx")]
    [TestCase("The Keys.pptx")]
    [TestCase("Main Story Act One.xlsx")]
    [TestCase("Main Story Act Two.csv")]
    [TestCase("Main Story Act Three.json")]
    public void RemoveTXTTest_DifferentFileTypes(string title)
    {
        // Create a game object and add the BookMaker script to it.
        GameObject testObject = createBookMakerObject();
        BookMaker bM = testObject.GetComponent<BookMaker>();
        
        bM.GetFiles();
        bM.setCurrentBook("The Bus And New Friends");

        // Call the RemoveTXT function.
        string result = bM.RemoveTXT(title);

        // Check to see if the .txt extension is removed.
        Assert.IsFalse(result.Contains(".txt"));
    }

    // Test MakeBook function
    [UnityTest]
    public IEnumerator MakeBookTest()
    {
        // Create a game object and add the BookMaker script to it.
        GameObject testObject = createBookMakerObject();
        BookMaker bM = testObject.GetComponent<BookMaker>();
        bM.isPracticeMode = true;

        bM.GetFiles();
        
        // Create a txt file and add it to the PracticeStories folder.
        string path = Application.streamingAssetsPath + "/Stories/PracticeStories/Test Text.txt";
        File.WriteAllText(path, "This is a test text file. It is a test text file! Okay? Okay.");

        // Call the MakeBook function.
        bM.MakeBook("Test Text.txt");

        // Wait for the book to be made.
        yield return null;

        // Delete the test file.
        File.Delete(path);

        // Check to see if the book was added to the bookshelf.
        Assert.IsTrue(bM.bookShelf.books.ContainsKey("Test Text"));

        // Check to see if the book has the correct number of pages.
        Assert.IsTrue(bM.bookShelf.getBookByName("Test Text").numPages == 4);

        // Check to see that the book was divided properly.
        Assert.IsTrue(bM.bookShelf.getBookByName("Test Text").text[0] == "This is a test text file.");
        Assert.IsTrue(bM.bookShelf.getBookByName("Test Text").text[1] == "It is a test text file!");
        Assert.IsTrue(bM.bookShelf.getBookByName("Test Text").text[2] == "Okay?");
        Assert.IsTrue(bM.bookShelf.getBookByName("Test Text").text[3] == "Okay.");
    }

    // Test creating a report.
    [Test]
    public void CreateReportAndCompareTextTest()
    {
        // Create a TimeSpan object to use for the report.
        TimeSpan time = new TimeSpan();

        string sentence1 = "This is a test sentence.";
        string sentence2 = "This is a sentence.";

        // Add the sentences to their own lists.
        List<string> sentences1 = new List<string>();
        sentences1.Add(sentence1);
        List<string> sentences2 = new List<string>();
        sentences2.Add(sentence2);

        GameObject testObject = createBookMakerObject();
        BookMaker bM = testObject.GetComponent<BookMaker>();

        // Create a report class from the BookMaker script.
        BookMaker.Report report = new BookMaker.Report(sentences1, sentences2, time);

        // Check to see if the report was created correctly.
        Assert.IsTrue(report.numCorrectWords == 4);
        Assert.IsTrue(report.numIncorrectWords == 1);
        Assert.IsTrue(report.numWords == 5);
        Assert.AreEqual(report.completionTime, "00:00:00.00");
    }
}

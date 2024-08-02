using System;
using NUnit.Framework;

namespace JohnsonControls.Metasys.BasicServices;

[TestFixture]
public class ObjectIdTests
{
    // Mainly just making sure we never get a Null exception due to the default constructor
    // This was really addressed by the fact that I switched ObjectId to be a record,
    // changed the language in csproj to 12 and provided a default constructor that ensures
    // we default to empty string
    [Test]
    public void DefaultInstanceNeverThrows()
    {
        // Arrange
        var id = new ObjectId();

        // Assertions
        Assert.That(id.ToString(), Is.EqualTo(""));
        Assert.That((string)id, Is.EqualTo(""));
        Assert.That(id == "", Is.True);
        Assert.That(id.GetHashCode(), Is.EqualTo(id.ToString().GetHashCode()));
    }


    // Basically make sure conversions to/from string are correct
    [TestCase("")]
    [TestCase("54efad4c-0fb7-59b4-9b6f-e385fd7e5b9e")]
    [TestCase("Happy Data 😃")]
    public void StringConversions(string inputString)
    {
        // Act
        ObjectId objectId = inputString;

        // Assert
        Assert.That(objectId, Is.EqualTo(inputString));
        Assert.That(objectId, Is.EqualTo(new ObjectId(inputString)));
        Assert.That(inputString, Is.EqualTo(objectId));
        Assert.That(inputString, Is.EqualTo(objectId.ToString()));
        Assert.That(objectId == inputString, Is.True);
        Assert.That(objectId != inputString, Is.False);
        Assert.That(objectId != "78f5c3ca-46e1-5f93-b06e-5711677bf933", Is.True);
    }

    [TestCase("54efad4c-0fb7-59b4-9b6f-e385fd7e5b9e")]
    public void GuidConversions(string inputString)
    {
        // Act
        var guid = new Guid(inputString);
        ObjectId objectId = guid;

        // Assert
        Assert.That(objectId, Is.EqualTo(guid));
        Assert.That(objectId, Is.EqualTo(new ObjectId(inputString)));
        Assert.That(guid, Is.EqualTo(objectId));
    }
}

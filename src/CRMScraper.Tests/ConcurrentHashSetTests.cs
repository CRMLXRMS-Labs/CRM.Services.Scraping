using CRMScraper.Library.Core.Entities;
using System.Collections.Generic;
using Xunit;

namespace CRMScraper.Tests
{
    public class ConcurrentHashSetTests
    {
        [Fact]
        public void Add_AddsUniqueItems_ReturnsTrueForNewItems()
        {
            // Arrange
            var set = new ConcurrentHashSet<int>();

            // Act
            bool addedFirst = set.Add(1);
            bool addedSecond = set.Add(2);
            bool addedDuplicate = set.Add(1);

            // Assert
            Assert.True(addedFirst);
            Assert.True(addedSecond);
            Assert.False(addedDuplicate); 
        }

        [Fact]
        public void Contains_ChecksIfItemExistsInSet_ReturnsCorrectResult()
        {
            // Arrange
            var set = new ConcurrentHashSet<string>();
            set.Add("hello");
            set.Add("world");

            // Act
            bool containsHello = set.Contains("hello");
            bool containsWorld = set.Contains("world");
            bool containsUnknown = set.Contains("unknown");

            // Assert
            Assert.True(containsHello); 
            Assert.True(containsWorld); 
            Assert.False(containsUnknown);
        }

        [Fact]
        public void Count_ReturnsCorrectNumberOfUniqueItems()
        {
            // Arrange
            var set = new ConcurrentHashSet<int>();
            set.Add(1);
            set.Add(2);
            set.Add(3);
            set.Add(1); 

            // Act
            int count = set.Count;

            // Assert
            Assert.Equal(3, count); 
        }

        [Fact]
        public void GetEnumerator_IteratesOverAllItems()
        {
            // Arrange
            var set = new ConcurrentHashSet<int>();
            set.Add(1);
            set.Add(2);
            set.Add(3);

            // Act
            var items = new List<int>(set); 

            // Assert
            Assert.Contains(1, items);
            Assert.Contains(2, items);
            Assert.Contains(3, items);
            Assert.Equal(3, items.Count);
        }

        [Fact]
        public void Enumerator_ReturnsSameItemsAsContainsMethod()
        {
            // Arrange
            var set = new ConcurrentHashSet<string>();
            set.Add("item1");
            set.Add("item2");
            set.Add("item3");

            // Act
            bool allItemsFound = true;
            foreach (var item in set)
            {
                if (!set.Contains(item))
                {
                    allItemsFound = false;
                    break;
                }
            }

            // Assert
            Assert.True(allItemsFound);
        }
    }
}

using MyBackend.Services;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using FirebaseAdmin;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth.OAuth2;
using System.IO;

namespace MyBackend.Tests
{
    public class SortAlgorithmServiceTests
    {
        private readonly SortAlgorithmService _sortAlgorithmService;
        private readonly FirestoreDb _firestoreDb;
        private const decimal Tolerance = 0.01m; // Define a tolerance for floating-point comparison
        private const string ProjectId = "cynapseinterview"; // Set your actual Firebase Project ID

        public SortAlgorithmServiceTests()
        {
            // Load configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Set the base path for the configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Load appsettings.json
                .Build();

            // Get the service account key path from configuration
            var serviceAccountPath = configuration["Firebase:ServiceAccountKeyPath"];
            
            if (string.IsNullOrEmpty(serviceAccountPath))
            {
                throw new InvalidOperationException("Service Account Key Path is not configured in appsettings.json.");
            }

            // Set the environment variable for Google application credentials
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", serviceAccountPath);

            // Initialize Firestore client
            _firestoreDb = FirestoreDb.Create(ProjectId); // Use your actual Firebase project ID
            _sortAlgorithmService = new SortAlgorithmService();
        }

        [Fact]
        public void Allocate_ShouldDistributeAmountCorrectly()
        {
            decimal totalAmount = 10m;
            int numberOfParticipants = 5;

            var result = _sortAlgorithmService.Allocate(totalAmount, numberOfParticipants);

            Assert.Equal(numberOfParticipants, result.Count); 
            Assert.True(Math.Abs(totalAmount - result.Sum(r => r.Value)) < Tolerance, "The sum of allocations does not match the total amount."); 
            Assert.All(result, r => Assert.True(r.Value >= 0)); 
        }

        [Fact]
        public void Allocate_ShouldThrowException_WhenInvalidInput()
        {
            Assert.Throws<ArgumentException>(() => _sortAlgorithmService.Allocate(0, 5)); 
            Assert.Throws<ArgumentException>(() => _sortAlgorithmService.Allocate(10, 0)); 
            Assert.Throws<ArgumentException>(() => _sortAlgorithmService.Allocate(-1, 5)); 
            Assert.Throws<ArgumentException>(() => _sortAlgorithmService.Allocate(10, -5)); 
        }

        [Theory]
        [InlineData(20, 4)]
        [InlineData(50, 10)]
        [InlineData(100, 1)]
        public void Allocate_ShouldWorkForVariousInputs(decimal totalAmount, int numberOfParticipants)
        {
            var result = _sortAlgorithmService.Allocate(totalAmount, numberOfParticipants);

            Assert.Equal(numberOfParticipants, result.Count); 
            Assert.True(Math.Abs(totalAmount - result.Sum(r => r.Value)) < Tolerance, "The sum of allocations does not match the total amount."); 
        }

        [Fact]
        public void Allocate_ShouldRoundToTwoDecimals()
        {
            decimal totalAmount = 100m;
            int numberOfParticipants = 3;

            var result = _sortAlgorithmService.Allocate(totalAmount, numberOfParticipants);

            Assert.All(result, r => Assert.True(Math.Abs(r.Value - Math.Round(r.Value, 2)) < Tolerance, "Allocation amounts are not rounded to two decimals."));
        }

        [Fact]
        public async void TestFireStorePersistence()
        {   
            var collectionName = "test_allocations";

            var testAllocation = new
            {
                Participant = "Participant 2",
                Amount = (double)10.00m, 
                Timestamp = Timestamp.GetCurrentTimestamp()
            };

            var collectionReference = _firestoreDb.Collection(collectionName);
            var documentReference = await collectionReference.AddAsync(testAllocation);
            var documentSnapshot = await documentReference.GetSnapshotAsync();

            Assert.True(documentSnapshot.Exists, "Document was not persisted in Firestore.");
            Assert.Equal(testAllocation.Participant, documentSnapshot.GetValue<string>("Participant"));
            Assert.Equal(testAllocation.AmountAsDouble, documentSnapshot.GetValue<double>("AmountAsDouble"));
        }
    }
}

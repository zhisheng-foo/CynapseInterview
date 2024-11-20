using MyBackend.Services;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using FirebaseAdmin;
using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using Grpc.Net.Client;
using System.Threading.Tasks;

namespace MyBackend.Tests
{
    public class SortAlgorithmServiceTests
    {
        private readonly SortAlgorithmService _sortAlgorithmService;
        private const decimal Tolerance = 0.01m; // Define a tolerance for floating-point comparison

        public SortAlgorithmServiceTests()
        {
            _sortAlgorithmService = new SortAlgorithmService();
        }

        [Fact]
        public void Allocate_ShouldDistributeAmountCorrectly()
        {
            decimal totalAmount = 10m;
            int numberOfParticipants = 5;

            var result = _sortAlgorithmService.Allocate(totalAmount, numberOfParticipants);

            Assert.Equal(numberOfParticipants, result.Count);
            Assert.True(Math.Abs(totalAmount - result.Sum(r => r.Value)) < Tolerance, 
                        "The sum of allocations does not match the total amount.");
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
            Assert.True(Math.Abs(totalAmount - result.Sum(r => r.Value)) < Tolerance, 
                        "The sum of allocations does not match the total amount.");
        }

        [Fact]
        public void Allocate_ShouldRoundToTwoDecimals()
        {
            decimal totalAmount = 100m;
            int numberOfParticipants = 3;

            var result = _sortAlgorithmService.Allocate(totalAmount, numberOfParticipants);

            Assert.All(result, r => 
                Assert.True(Math.Abs(r.Value - Math.Round(r.Value, 2)) < Tolerance, 
                            "Allocation amounts are not rounded to two decimals."));
        }
        [Fact]
        public async Task TestFireStorePersistence()
        {
            const string collectionName = "test_allocations";
            const string projectId = "cynapseinterview"; 


            const string serviceAccountKeyPath = "C:/Users/User/Desktop/CynapseInterview/backend/ServiceAccountKey.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", serviceAccountKeyPath);

            // Authenticate using the service account credentials via environment variable
            var firestoreDb = FirestoreDb.Create(projectId);
            Console.WriteLine($"Connected to Firestore project: {firestoreDb.ProjectId}");

            // Data to add
            var testAllocation = new
            {
                Participant = "Participant 2",
                Amount = 10.00, // Use double directly
                Timestamp = Timestamp.GetCurrentTimestamp()
            };

            // Perform Firestore operations
            var collectionReference = firestoreDb.Collection(collectionName);
            Console.WriteLine($"Collection reference: {collectionReference.Path}");

            var documentReference = await collectionReference.AddAsync(testAllocation);
            Console.WriteLine($"Document added with ID: {documentReference.Id}");

            var documentSnapshot = await documentReference.GetSnapshotAsync();

            // Assertions
            Assert.True(documentSnapshot.Exists, "Document was not persisted in Firestore.");
            Assert.Equal(testAllocation.Participant, documentSnapshot.GetValue<string>("Participant"));
            Assert.Equal(testAllocation.Amount, documentSnapshot.GetValue<double>("Amount"));

            Console.WriteLine("Firestore persistence test passed!");
        }   
    }

}

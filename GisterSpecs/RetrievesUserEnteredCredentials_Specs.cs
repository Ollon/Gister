﻿using System.Windows.Forms;
using EchelonTouchInc.Gister;
using NUnit.Framework;
using Should.Fluent;

namespace GisterSpecs
{
    [TestFixture]
    public class RetrievesUserEnteredCredentials_Specs
    {
        [Test]
        public void WillApplyCredentialsIfUserOks()
        {
            var applies = new RetrievesUserEnteredCredentials();

            applies.CreatePrompt = () =>
                                       {
                                           var prompt = new MockCredentialPrompt();

                                           prompt.RespondWith("it", "rocks");
                                           prompt.Result = true;

                                           return prompt;
                                       };

            var receiver = applies.Retrieve();

            receiver.Username.Should().Equal("it");
            receiver.Password.Should().Equal("rocks");
        }

        [Test]
        public void WillNotApplyCredentialsIfUserCancels()
        {
            var applies = new RetrievesUserEnteredCredentials();

            applies.CreatePrompt = () =>
                                       {
                                           var prompt = new MockCredentialPrompt();

                                           prompt.RespondWith("it", "didn't");
                                           prompt.Result = false;

                                           return prompt;
                                       };

            var receiver = applies.Retrieve();

            receiver.Username.Should().Not.Equal("it");
        }
    }

    public class MockCredentialPrompt : CredentialsPrompt
    {
        public bool? Result { get; set; }

        public void RespondWith(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public void Prompt()
        {
        }

        public string Username { get; private set; }
        public string Password { get; private set; }
    }
}
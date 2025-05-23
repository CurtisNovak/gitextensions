﻿using FluentAssertions;
using GitCommands;
using GitCommands.Config;
using GitExtensions.Extensibility.Git;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitUITests.UserControls
{
    [TestFixture]
    internal class AuthorRevisionHighlightingTests
    {
        private const string ExpectedAuthorEmail1 = "doe1@example.org";
        private const string ExpectedAuthorEmail2 = "doe2@example.org";

        [Test]
        public void AuthorEmailToHighlight_should_be_null_when_no_revision_change_processed_yet()
        {
            AuthorRevisionHighlighting sut = new();

            sut.AuthorEmailToHighlight.Should().BeNull();
        }

        [Test]
        public void When_multiple_revisions_selected_Then_ProcessSelectionChange_should_return_NoAction()
        {
            AuthorRevisionHighlighting sut = new();
            GitModule currentModule = NewModule();

            bool action = sut.ProcessRevisionSelectionChange(currentModule,
                                               new[]
                                                   {
                                                       NewRevisionWithAuthorEmail(ExpectedAuthorEmail1),
                                                       NewRevisionWithAuthorEmail(ExpectedAuthorEmail1)
                                                   });

            action.Should().Be(false);
        }

        [Test]
        public void Given_previously_selected_revision_When_multiple_revisions_selected_Then_AuthorEmailToHighlight_should_not_change()
        {
            AuthorRevisionHighlighting sut = new();
            GitModule currentModule = NewModule();
            ClassicAssert.True(sut.ProcessRevisionSelectionChange(currentModule,
                                               new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) }));

            ClassicAssert.False(sut.ProcessRevisionSelectionChange(currentModule,
                                               new[]
                                                   {
                                                       NewRevisionWithAuthorEmail(ExpectedAuthorEmail2),
                                                       NewRevisionWithAuthorEmail(ExpectedAuthorEmail1)
                                                   }));

            sut.AuthorEmailToHighlight.Should().Be(ExpectedAuthorEmail1);
        }

        [Test]
        public void Given_no_previously_selected_revision_When_single_revision_selected_Then_ProcessSelectionChange_should_return_RefreshUserInterface()
        {
            AuthorRevisionHighlighting sut = new();
            GitModule currentModule = NewModule();

            bool action = sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) });

            action.Should().Be(true);
        }

        [Test]
        public void Given_no_previously_selected_revision_When_single_revision_selected_Then_AuthorEmailToHighlight_should_change()
        {
            AuthorRevisionHighlighting sut = new();
            GitModule currentModule = NewModule();

            sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) });

            sut.AuthorEmailToHighlight.Should().Be(ExpectedAuthorEmail1);
        }

        [Test]
        public void Given_previously_selected_revision_When_single_revision_with_same_author_email_selected_Then_ProcessSelectionChange_should_return_NoAction()
        {
            AuthorRevisionHighlighting sut = new();
            GitModule currentModule = NewModule();
            sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) });

            bool action = sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) });

            action.Should().Be(false);
        }

        [Test]
        public void Given_previously_selected_revision_When_single_revision_with_same_author_email_selected_Then_AuthorEmailToHighlight_should_not_change()
        {
            AuthorRevisionHighlighting sut = new();
            GitModule currentModule = NewModule();
            sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) });

            sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) });

            sut.AuthorEmailToHighlight.Should().Be(ExpectedAuthorEmail1);
        }

        [Test]
        public void Given_previously_selected_revision_When_single_revision_with_different_author_email_selected_Then_ProcessSelectionChange_should_return_RefreshUserInterface()
        {
            AuthorRevisionHighlighting sut = new();
            GitModule currentModule = NewModule();
            sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) });

            bool action = sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail2) });

            action.Should().Be(true);
        }

        [Test]
        public void Given_previously_selected_revision_When_single_revision_with_different_author_email_selected_Then_AuthorEmailToHighlight_should_change()
        {
            AuthorRevisionHighlighting sut = new();
            GitModule currentModule = NewModule();
            sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) });

            sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail2) });

            sut.AuthorEmailToHighlight.Should().Be(ExpectedAuthorEmail2);
        }

        [Test]
        public void Given_previously_selected_revision_When_no_revision_selected_Then_ProcessSelectionChange_should_return_RefreshUserInterface()
        {
            AuthorRevisionHighlighting sut = new();
            IGitModule currentModule = Substitute.For<IGitModule>();
            currentModule.GetEffectiveSetting(SettingKeyString.UserEmail).Returns(ExpectedAuthorEmail2);
            sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) });

            bool action = sut.ProcessRevisionSelectionChange(currentModule, Array.Empty<GitRevision>());

            action.Should().Be(true);
        }

        [Test]
        public void Given_previously_selected_revision_When_no_revision_selected_Then_AuthorEmailToHighlight_should_be_value_of_current_user_email_setting()
        {
            AuthorRevisionHighlighting sut = new();
            IGitModule currentModule = Substitute.For<IGitModule>();
            currentModule.GetEffectiveSetting(SettingKeyString.UserEmail).Returns(ExpectedAuthorEmail2);
            sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) });

            sut.ProcessRevisionSelectionChange(currentModule, Array.Empty<GitRevision>());

            sut.AuthorEmailToHighlight.Should().Be(ExpectedAuthorEmail2);
        }

        [Test]
        public void IsHighlighted_should_return_false_if_revision_is_null()
        {
            AuthorRevisionHighlighting sut = new();

            sut.IsHighlighted(null).Should().BeFalse();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void IsHighlighted_should_return_false_if_revision_AuthorEmail_is_null_or_whitespace(string authorEmail)
        {
            AuthorRevisionHighlighting sut = new();

            sut.IsHighlighted(new GitRevision(ObjectId.Random()) { AuthorEmail = authorEmail }).Should().BeFalse();
        }

        [TestCase("a@a.aaa", "a@a.aaa", true)]
        [TestCase("A@A.aaa", "a@a.aaa", true)]
        [TestCase("b@A.aaa", "a@a.aaa", false)]
        [TestCase("a@a.aaa", null, false)]
        [TestCase("a@a.aaa", "", false)]
        [TestCase("a@a.aaa", "\t", false)]
        public void IsHighlighted_should_return_true_if_revision_AuthorEmail_matches_AuthorEmailToHighlight(string authorEmail, string highlightEmail, bool expected)
        {
            GitModule currentModule = NewModule();
            AuthorRevisionHighlighting sut = new();
            sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(highlightEmail) });
            sut.AuthorEmailToHighlight.Should().Be(highlightEmail);

            sut.IsHighlighted(new GitRevision(ObjectId.Random()) { AuthorEmail = authorEmail }).Should().Be(expected);
        }

        private static GitModule NewModule()
        {
            return new GitModule(Path.GetTempPath());
        }

        private static GitRevision NewRevisionWithAuthorEmail(string authorEmail)
        {
            return new GitRevision(ObjectId.Random())
            {
                AuthorEmail = authorEmail
            };
        }
    }
}

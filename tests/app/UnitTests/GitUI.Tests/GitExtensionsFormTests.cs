﻿using FluentAssertions;
using GitExtUtils.GitUI;
using GitUI;
using NSubstitute;

namespace GitUITests
{
    [Apartment(ApartmentState.STA)]
    [TestFixture]
    public class GitExtensionsFormTests
    {
        private IWindowPositionManager _windowPositionManager;

        [SetUp]
        public void Setup()
        {
            _windowPositionManager = Substitute.For<IWindowPositionManager>();
        }

        [Test]
        public void RestorePosition_should_not_restore_position_if_not_required()
        {
            using MockForm form = new(false)
            {
                Location = new Point(-100, -100),
                Size = new Size(500, 500)
            };

            GitExtensionsForm.GitExtensionsFormTestAccessor test = form.GetGitExtensionsFormTestAccessor();
            test.GetScreensWorkingArea = () => throw new InvalidOperationException();

            form.InvokeRestorePosition();

            form.Location.Should().Be(new Point(-100, -100));
            form.Size.Should().Be(new Size(500, 500));
        }

        [Test]
        public void RestorePosition_should_not_restore_position_if_minimised()
        {
            using MockForm form = new(false)
            {
                Location = new Point(-100, -100),
                Size = new Size(500, 500),
                WindowState = FormWindowState.Minimized
            };

            GitExtensionsForm.GitExtensionsFormTestAccessor test = form.GetGitExtensionsFormTestAccessor();
            test.GetScreensWorkingArea = () => throw new InvalidOperationException();

            form.InvokeRestorePosition();

            form.Location.Should().Be(new Point(-100, -100));
            form.Size.Should().Be(new Size(500, 500));
            form.WindowState.Should().Be(FormWindowState.Minimized);
        }

        [Test]
        public void RestorePosition_should_not_restore_position_if_not_persisted()
        {
            using MockForm form = new(true)
            {
                Location = new Point(-100, -100),
                Size = new Size(500, 500)
            };

            GitExtensionsForm.GitExtensionsFormTestAccessor test = form.GetGitExtensionsFormTestAccessor();
            test.GetScreensWorkingArea = () => throw new InvalidOperationException();
            test.WindowPositionManager = _windowPositionManager;

            _windowPositionManager.LoadPosition(form).Returns(x => null);

            form.InvokeRestorePosition();

            form.Location.Should().Be(new Point(-100, -100));
            form.Size.Should().Be(new Size(500, 500));
        }

        [TestCase(FormBorderStyle.Fixed3D)]
        [TestCase(FormBorderStyle.FixedDialog)]
        [TestCase(FormBorderStyle.FixedSingle)]
        [TestCase(FormBorderStyle.FixedToolWindow)]
        [TestCase(FormBorderStyle.None)]
        public void RestorePosition_should_not_scale_fixed_window_if_different_dpi(FormBorderStyle borderStyle)
        {
            using MockForm form = new(true)
            {
                Location = new Point(-100, -100),
                Size = new Size(300, 300),
                FormBorderStyle = borderStyle
            };
            Rectangle[] screens = new[]
            {
                new Rectangle(-1920, 0, 1920, 1080),
                new Rectangle(1920, 0, 1920, 1080),
                new Rectangle(0, 0, 1920, 1080)
            };

            GitExtensionsForm.GitExtensionsFormTestAccessor test = form.GetGitExtensionsFormTestAccessor();
            test.GetScreensWorkingArea = () => screens;
            test.WindowPositionManager = _windowPositionManager;

            _windowPositionManager.LoadPosition(form)
                .Returns(x => new WindowPosition(new Rectangle(100, 100, 500, 500), 96, FormWindowState.Normal, "bla"));

            form.InvokeRestorePosition();

            form.Size.Should().Be(new Size(300, 300));
        }

        [TestCase(96, 500, 500)]
        [TestCase(120, 400, 400)]
        [TestCase(144, 333, 333)]
        [TestCase(192, 250, 250)]
        public void RestorePosition_should_scale_sizable_window_if_different_dpi(int savedDpi, int expectedWidthAt96dpi, int expectedHeightAt96dpi)
        {
            if (DpiUtil.IsNonStandard)
            {
                ClassicAssert.Inconclusive("The test must be run at 96dpi");
            }

            using MockForm form = new(true)
            {
                Location = new Point(-100, -100),
                Size = new Size(300, 300),
            };
            Rectangle[] screens = new[]
            {
                new Rectangle(-1920, 0, 1920, 1080),
                new Rectangle(1920, 0, 1920, 1080),
                new Rectangle(0, 0, 1920, 1080)
            };

            GitExtensionsForm.GitExtensionsFormTestAccessor test = form.GetGitExtensionsFormTestAccessor();
            test.GetScreensWorkingArea = () => screens;
            test.WindowPositionManager = _windowPositionManager;

            _windowPositionManager.LoadPosition(form)
                .Returns(x => new WindowPosition(new Rectangle(100, 100, 500, 500), savedDpi, FormWindowState.Normal, "bla"));

            form.InvokeRestorePosition();

            form.Size.Should().Be(new Size(expectedHeightAt96dpi, expectedHeightAt96dpi));
        }

        [TestCase(-1000, 100, /* -1000 + (800 - 300)/2 */ -750, /* 100 + (600-200)/2 */300)]
        [TestCase(0, 0, /* 0 + (800 - 300)/2 */ 250, /* 0 + (600-200)/2 */200)]
        [TestCase(1000, -500, /* falls off the screen -> first non-empty screen */ -1920, /* falls off the screen */ 0)]
        public void RestorePosition_should_position_window_with_Owner_set_and_CenterParent(int ownerFormTop, int ownerFormLeft, int expectFormTop, int expectedFormLeft)
        {
            if (DpiUtil.IsNonStandard)
            {
                ClassicAssert.Inconclusive("The test must be run at 96dpi");
            }

            using Form owner = new()
            {
                Location = new Point(ownerFormTop, ownerFormLeft),
                Size = new Size(800, 600)
            };
            using MockForm form = new(true)
            {
                Owner = owner,
                StartPosition = FormStartPosition.CenterParent
            };
            Rectangle[] screens = new[]
            {
                new Rectangle(0, 0, 0, 0),
                new Rectangle(-1920, 0, 1920, 1080),
                new Rectangle(1920, 0, 1920, 1080),
                new Rectangle(0, 0, 1920, 1080)
            };

            GitExtensionsForm.GitExtensionsFormTestAccessor test = form.GetGitExtensionsFormTestAccessor();
            test.GetScreensWorkingArea = () => screens;
            test.WindowPositionManager = _windowPositionManager;
            _windowPositionManager.LoadPosition(form)
                .Returns(x => new WindowPosition(new Rectangle(100, 100, 300, 200), 96, FormWindowState.Normal, "bla"));

            form.InvokeRestorePosition();

            form.Location.Should().Be(new Point(expectFormTop, expectedFormLeft));
        }

        private class MockForm : GitExtensionsForm
        {
            public MockForm(bool enablePositionRestore)
                : base(enablePositionRestore)
            {
            }

            public void InvokeRestorePosition()
            {
                RestorePosition();
            }
        }
    }
}

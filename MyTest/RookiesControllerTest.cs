using RookiesMVC.Controllers;
using NUnit.Framework;
using RookiesMVC.Service;
using Microsoft.Extensions.Logging;
using RookiesMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace RookiesControllerTest;

public class Tests
{
    private RookiesController _rookiesController;
    private Mock<IPersonService> _personServiceMock;
    private Mock<ILogger<RookiesController>> _loggerMock;
    private static List<PersonModel> _data = new List<PersonModel>{
        new PersonModel
        {
            FirstName = "Tien",
            LastName = "Nguyen",
            BirthPlace = "Ha Noi"
        }
    };

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<RookiesController>>();
        _personServiceMock = new Mock<IPersonService>();
        _rookiesController = new RookiesController(_loggerMock.Object, _personServiceMock.Object);
        _personServiceMock.Setup(q => q.GetAll()).Returns(_data);
    }

    [Test]
    public void GetAllMember_Success()
    {
        var result = _rookiesController.Index();

        Assert.IsInstanceOf<ViewResult>(result);

        var view = (ViewResult)result;

        Assert.IsInstanceOf<List<PersonModel>>(view.ViewData.Model);
        Assert.IsAssignableFrom<List<PersonModel>>(view.ViewData.Model);

        var list = (List<PersonModel>)view.ViewData.Model;

        Assert.AreEqual(1, list.Count());
    }

    [Test]
    public void Create_RedirectToAction()
    {
        _rookiesController.ModelState.AddModelError("FirstName", "Required");

        var result = _rookiesController.Create(model: null);

        Assert.IsInstanceOf<ViewResult>(result);
    }

    [Test]
    public void Create_ReturnView()
    {
        var newCreatePerson = new PersonCreateModel()
        {
            FirstName = "Tien",
        };
        var result = _rookiesController.Create(newCreatePerson);

        Assert.IsInstanceOf<RedirectToActionResult>(result);

        var actual = (RedirectToActionResult)result;

        Assert.AreEqual("Index", actual.ActionName);
    }

    [Test]
    public void UpdatePost_ReturnBadRequest_StateIsValid()
    {
        _rookiesController.ModelState.AddModelError("FirstName", "FieldRequired");

        var member = new PersonModel();
        var index = 1;
        var updatePerson = new PersonUpdateModel();
        var result = _rookiesController.Update(index, updatePerson);

        Assert.IsInstanceOf<BadRequestObjectResult>(result);

        var badRequestResult = (BadRequestObjectResult)result;
        var serialize = (SerializableError)badRequestResult.Value;

        Assert.AreEqual("FirstName", serialize.Keys.ToList()[0] as string);
    }

    [Test]
    public void UpdatePost_RedirectToAction_StateIsValid()
    {
        var member = new PersonModel();
        var index = 3;
        var updateMember = new PersonUpdateModel()
        {
            FirstName = "T"
        };

        var result = _rookiesController.Update(index, updateMember);

        Assert.IsInstanceOf<RedirectToActionResult>(result);

        var redirectToActionResult = (RedirectToActionResult)result;

        Assert.Null(redirectToActionResult.ControllerName);
        Assert.AreEqual("Index", redirectToActionResult.ActionName);
    }

    [Test]
    public void DeleteOnePerson_Test(Mock<IPersonService> _personServiceMock)
    {
        int index = 1;
        _personServiceMock.Setup(expression: p => p.Delete(index)).Callback(() =>
        {
            _data.Remove(_data[1]);
        }).Returns(_data[1]);

        var controller = new RookiesController(_loggerMock.Object, _personServiceMock.Object);
        var expected = _data.Count - 1;

        var result = controller.Delete(2);
        var actual = _data.Count;

        Assert.IsInstanceOf<RedirectToActionResult>(result);
        Assert.IsNotNull(result);
        Assert.AreEqual(expected, actual);
    }
}
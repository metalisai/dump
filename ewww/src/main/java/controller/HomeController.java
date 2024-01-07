package controller;

import java.util.List;

import javax.servlet.http.HttpServletRequest;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Controller;
import org.springframework.ui.ModelMap;
import org.springframework.validation.BindingResult;
import org.springframework.web.bind.annotation.ModelAttribute;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RequestParam;

import dao.PersonDao;
import dao.SetupDao;
import model.Person;
import model.Phone;
import view.CustomerForm;

@Controller
public class HomeController {

	@Autowired
	PersonDao personDao;
	
	@RequestMapping("/search")
	public String searchUnit(ModelMap model,
			@RequestParam(value = "searchString", required = false) 
			String searchString) 
	{
		
		List<Person> people;
		
		if(searchString != null && !searchString.isEmpty())
		{
			people = personDao.searchPeopleByName(searchString);
		}
		else
		{
			people = personDao.findAllPersons();
		}
		
		model.addAttribute("people", people);
		
		return "Search";
	}
	
	@RequestMapping(value="/delete/{customerCode}")
	public String deleteUnit(@PathVariable("customerCode") String customerCode,
			@ModelAttribute("customerForm") CustomerForm form) 
	{
		personDao.removePersonByCode(customerCode);
		
		return "redirect:/search";
	}
	
	@RequestMapping(value="/view/{customerCode}")
	public String viewUnit(@PathVariable("customerCode") String customerCode,
			@ModelAttribute("customerForm") CustomerForm form) 
	{
		Person customer = personDao.getPersonByCode(customerCode);
		
		form.setPerson(customer);
		form.setCustomerTypes(personDao.getCustomerTypes());
		form.setPhoneTypes(personDao.getPhoneTypes());
		form.setFormDisabled(true);	
		
		return "Add";
	}
	
	@RequestMapping(value = "/addForm", method = RequestMethod.GET)
	public String addGetUnit(@ModelAttribute("customerForm") CustomerForm form) 
	{
		form.setCustomerTypes(personDao.getCustomerTypes());
		return "Add";
	}
	
	@RequestMapping(value = "/addForm", method = RequestMethod.POST)
	public String addPostUnit(@ModelAttribute("customerForm") CustomerForm form,
			BindingResult result) 
	{
		Phone pressed;
		
		if(form.getAddPhoneButton() != null)
		{
			form.getPerson().addPhone(new Phone());
			form.setCustomerTypes(personDao.getCustomerTypes());
			form.setPhoneTypes(personDao.getPhoneTypes());
			return "Add";
		}
		else if((pressed = form.getPerson().getPhoneWithDeletePressed()) != null) // ugly, but I'm lazy
		{
			form.getPerson().getPhones().remove(pressed);
			form.setCustomerTypes(personDao.getCustomerTypes());
			form.setPhoneTypes(personDao.getPhoneTypes());
			return "Add";
		}
		else
		{
			personDao.insertPerson(form.getPerson());
		}
		
		return "redirect:/search";
	}
	
	@RequestMapping("/admin")
	public String adminUnit(HttpServletRequest request)
	{
		String action = request.getParameter("do");
		switch(action)
		{
		case "clear_data":
			new SetupDao().executeSqlFile("schema.sql");
			break;
		case "insert_data":
			new SetupDao().executeSqlFile("testdata.sql");
			break;
		default:
			break;
		}
		
		return "redirect:/search";
	}
}
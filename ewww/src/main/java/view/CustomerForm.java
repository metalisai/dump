package view;

import java.util.Map;

import model.Person;

public class CustomerForm {
	private Person person;
	
	private Map<String,String> customerTypes;
	private Map<String,String> phoneTypes;
	
	private boolean formDisabled = false;
	private String addPhoneButton;

	public Person getPerson() {
		return person;
	}

	public void setPerson(Person person) {
		this.person = person;
	}

	public Map<String,String> getCustomerTypes() {
		return customerTypes;
	}

	public void setCustomerTypes(Map<String,String> customerTypes) {
		this.customerTypes = customerTypes;
	}

	public boolean getFormDisabled() {
		return formDisabled;
	}

	public void setFormDisabled(boolean formDisabled) {
		this.formDisabled = formDisabled;
	}

	public String getAddPhoneButton() {
		return addPhoneButton;
	}

	public void setAddPhoneButton(String addPhoneButton) {
		this.addPhoneButton = addPhoneButton;
	}

	public Map<String,String> getPhoneTypes() {
		return phoneTypes;
	}

	public void setPhoneTypes(Map<String,String> phoneTypes) {
		this.phoneTypes = phoneTypes;
	}
}

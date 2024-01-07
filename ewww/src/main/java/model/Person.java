package model;

import java.util.ArrayList;
import java.util.List;

import javax.persistence.CascadeType;
import javax.persistence.Entity;
import javax.persistence.FetchType;
import javax.persistence.OneToMany;

@Entity
public class Person extends BaseEntity
{
	String code;
	String firstName;
	String lastName;
	String customerType;
	
	@OneToMany(fetch = FetchType.EAGER,
			cascade = CascadeType.ALL)
	private List<Phone> phones = new ArrayList<Phone>();

	public Person(){}
	
	public Person(String code, String firstName, String lastName, String customerType) {
		super();
		this.code = code;
		this.firstName = firstName;
		this.lastName = lastName;
		this.customerType = customerType;
	}
	
	public String getCustomerType() {
		return customerType;
	}

	public void setCustomerType(String customerType) {
		this.customerType = customerType;
	}
	
	public String getFirstName() {
		return firstName;
	}
	public void setFirstName(String firstName) {
		this.firstName = firstName;
	}
	public String getLastName() {
		return lastName;
	}
	public void setLastName(String lastName) {
		this.lastName = lastName;
	}
	public String getCode() {
		return code;
	}
	public void setCode(String code) {
		this.code = code;
	}
	
	public List<Phone> getPhones() {
        return phones;
    }
    public void setPhones(List<Phone> phones) {
        this.phones = phones;
    }
    public void addPhone(Phone phone) {
        phones.add(phone);
    }
    public Phone getPhoneWithDeletePressed() {
        for (Phone phone : phones) {
            if (phone.getDeleteButton() != null) {
                return phone;
            }
        }

        return null;
    }
    public void removePhone(Phone phone) {
        phones.remove(phone);
    }
}

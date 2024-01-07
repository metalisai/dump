package dao;

import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;
import javax.persistence.Query;
import javax.persistence.TypedQuery;

import org.springframework.stereotype.Repository;
import org.springframework.transaction.annotation.Transactional;

import model.Person;

@Repository
public class PersonDao {
	
	@PersistenceContext
	private EntityManager em;
	
	public List<Person> findAllPersons()
	{
		return em.createQuery("select p from Person p",Person.class).getResultList();
	}
	
	public List<Person> searchPeopleByName(String name)
	{
		TypedQuery<Person> query = em.createQuery("SELECT p from Person p WHERE LOWER(p.firstname) LIKE :name OR LOWER(lastname) LIKE :name", Person.class);
		return query.getResultList();
	}
	
	@Transactional
	public void insertPerson(Person person)
	{
		em.persist(person);
	}
	
	@Transactional
	public void removePersonByCode(String code)
	{
		Query query = em.createQuery("DELETE from Person p WHERE p.code=:code");
		query.setParameter("code", code);
		query.executeUpdate();
	}
	
	public Person getPersonByCode(String code)
	{
		TypedQuery<Person> query = em.createQuery("SELECT p from Person p WHERE p.code LIKE :code", Person.class);
		query.setParameter("code", code);
		List<Person> result = query.getResultList();
		if(!result.isEmpty())
			return result.get(0);
		else
			return null;
	}
	
	public Map<String, String> getCustomerTypes() {
       Map<String, String> map = new LinkedHashMap<String, String>();
       map.put("customerType.private", "Private");
       map.put("customerType.corporate", "Corporate");
       return map;
   }
	
	public Map<String, String> getPhoneTypes() {
        Map<String, String> map = new LinkedHashMap<String, String>();
        map.put("phoneType.fixed", "Fixed");
        map.put("phoneType.mobile", "Mobile");
        return map;
    }
	
}

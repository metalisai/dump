package model;

import javax.persistence.*;

@Entity
public class Phone extends BaseEntity {

    private String value;
    private String type;

    @Transient
    private String deleteButton;

    public Phone(String number) {
        this.value = number;
    }

    public Phone(Long id) {
        setId(id);
    }

    public Phone() {}

    public String getValue() {
        return value;
    }

    public void setValue(String number) {
        this.value = number;
    }

    public String getDeleteButton() {
        return deleteButton;
    }

    public void setDeleteButton(String deleteButton) {
        this.deleteButton = deleteButton;
    }

    @Override
    public String toString() {
        return getId() + " - " + value;
    }

	public String getType() {
		return type;
	}

	public void setType(String type) {
		this.type = type;
	}

}

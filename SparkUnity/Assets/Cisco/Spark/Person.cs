using System;
using System.Collections.Generic;
using Cisco.Spark;

public class Person : SparkObject {

	public Person(string id) {
		Id = id;
	}

    internal override SparkType SparkType
    {
        get
        {
            return SparkType.Person;
        }
    }

    protected override Dictionary<string, object> ToDict(List<string> fields = null)
    {
        throw new NotImplementedException();
    }
}
